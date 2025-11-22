using CAP;
using MessagingContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
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

        [HttpGet("AvailableRegions")]
        [OutputCache(Duration = 30)]
        public async Task<IActionResult> GetAvailableRegions()
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var res = await bus.InvokeAsync<AlertAreaResponse>(new AlertAreaRequest());
            return Ok(res.regions);
        }


    }
}
