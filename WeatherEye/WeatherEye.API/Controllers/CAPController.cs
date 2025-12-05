using CAP;
using CAP.DTOs;
using MessagingContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using WeatherEye.API.Helpers;
using Wolverine;
using Wolverine.Runtime.Handlers;

namespace WeatherEye.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CAPController : ControllerBase
    {
        private readonly ILogger<CAPController> _logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        public CAPController(ILogger<CAPController> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        [HttpGet("Alarms")]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> Get([FromQuery] string language = "cs", [FromQuery] string region = "")
        {
            _logger.LogInformation("CAPController Get method called.");

            using IServiceScope scope = serviceScopeFactory.CreateScope();

            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();
            //CaptureCascadingMessages
            var res = await bus.InvokeAsync<AlertResponse>(new AlertRequest());

            if (string.IsNullOrWhiteSpace(region))
                return Ok(res.records.Where(x => x.Language == language));

            return Ok(res.records.Where(x => x.Language == language && x.AreaDesc == region));

        }

        [HttpGet("UserSpecificAlarms")]
        [OutputCache(Duration = 30)]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AlertInfoDTO>>> GetUserSpecificAlerts([FromQuery] string language = "cs")
        {
            string? email, username;

            AuthHelper.GetParametersFromAuthHeader(Request, out email, out username);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Missing username or email in token claims. Username: {Username}, Email: {Email}", username, email);
                return BadRequest("Missing username or email in token claims.");
            }

            using IServiceScope scope = serviceScopeFactory.CreateScope();

            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var userResponse = await bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email));


            var alertsResponse = await bus.InvokeAsync<AlertResponse>(new AlertRequest());
            var alerts = alertsResponse.records.Where(x => x.Language == "cs");


            var userPreferencesResponse = await bus.InvokeAsync<AlertPreferencesResponse>(new AlertPreferencesRequest(userResponse.UserOid));
            var userPreferences = userPreferencesResponse.AlertPreferences.Where(x => x.EmailNotification).ToList();

            HashSet<AlertInfoDTO> relevantAlerts = new();

            foreach (var pref in userPreferences)
            {
                var alertQuery = alerts.Where(a => a.AreaDesc == pref.AreaDesc);

                //TODO: dořešit ostatní fitry, které ukládáme v preferencích
                //if(string.IsNullOrWhiteSpace(pref.SpecificAreaDesc) == false)
                //{
                //    alertQuery = alertQuery.Where(a => a.SpecificAreaDesc == pref.SpecificAreaDesc);
                //}

                foreach (var alertInfo in alertQuery)
                {
                    relevantAlerts.Add(alertInfo);
                }

            }

            return relevantAlerts.ToList();
        }

        [HttpGet("AvailableRegions")]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> GetAvailableRegions()
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var res = await bus.InvokeAsync<AlertAreaResponse>(new AlertAreaRequest());
            return Ok(res.regions);
        }

        [HttpGet("AvailableSpecificRegions")]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> GetAvailableSpecificRegions([FromQuery] string AreaDesc)
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var res = await bus.InvokeAsync<AlertSpecificAreaResponse>(new AlertSpecificAreaRequest(AreaDesc));
            return Ok(res.specificRegions);
        }

        [HttpGet("AllAvailableSpecificRegions")]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> GetAllAvailableSpecificRegions()
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var res = await bus.InvokeAsync<AlertAllSpecificAreasResponse>(new AlertAllSpecificAreasRequest());
            return Ok(res.specificRegions);
        }
    }
}
