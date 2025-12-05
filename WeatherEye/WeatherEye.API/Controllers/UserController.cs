using MessagingContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Preferences.DTO;
using System.IdentityModel.Tokens.Jwt;
using WeatherEye.API.Helpers;
using Wolverine;

namespace WeatherEye.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        public UserController(ILogger<UserController> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        [HttpGet("UserOid")]
        public async Task<IActionResult> GetUserOid()
        {
            _logger.LogInformation("UserController GetUserOid method called");

            string? email, username;
            AuthHelper.GetParametersFromAuthHeader(Request, out email, out username);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Missing username or email in token claims. Username: {Username}, Email: {Email}", username, email);
                return BadRequest("Missing username or email in token claims.");
            }

            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();
            try
            {
                var response = await bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email));
                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GetUserOid for Username: {Username}, Email: {Email}", username, email);
                return new StatusCodeResult(500);
            }
        }

        [HttpPost("Preferences")]
        public async Task<IActionResult> SavePreferences(AlertPreferenceDTO preference)
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
            try
            {
                var UserResponse = await bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email));

                if (UserResponse?.UserOid is null || UserResponse?.UserOid == Guid.Empty)
                    return BadRequest("Neplatný uživatel");

                var response = await bus.InvokeAsync<SaveAlertPreferenceResponse>(new MessagingContracts.SaveAlertPreferenceRequest(
                    UserResponse?.UserOid ?? Guid.Empty,
                    preference.AreaDesc,
                    preference.SpecificAreaDesc,
                    preference.EmailNotification,
                    preference.InAppNotification,
                    preference.AlertInfoCertainty,
                    preference.AlertInfoSeverity

                    ));
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GetUserOid for Username: {Username}, Email: {Email}", username, email);
                return new StatusCodeResult(500);
            }
        }

        [HttpDelete("Preferences")]
        public async Task<IActionResult> DeletePreferences([FromQuery]Guid PreferenceOid)
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

            var resp = await bus.InvokeAsync<RemoveAlertPreferenceResponse>(new MessagingContracts.RemoveAlertPreferenceRequest(userResponse.UserOid, PreferenceOid));

            return Ok(resp);
        }

        [HttpGet("Preferences")]
        public async Task<IActionResult> Preferences()
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
            try
            {
                var UserResponse = await bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email));

                if (UserResponse?.UserOid is null || UserResponse?.UserOid == Guid.Empty)
                    return BadRequest("Neplatný uživatel");


                var response = await bus.InvokeAsync<AlertPreferencesResponse>(new MessagingContracts.AlertPreferencesRequest(UserResponse!.UserOid));
                return Ok(response.AlertPreferences?.ToArray() ?? []);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GetUserOid for Username: {Username}, Email: {Email}", username, email);
                return new StatusCodeResult(500);
            }
        }

        [HttpPost("SaveMobAppId")]
        public IActionResult SaveMobAppId(string MobAppId)
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
            try
            {
                var UserResponse = bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email)).Result;
                if (UserResponse?.UserOid is null || UserResponse?.UserOid == Guid.Empty)
                    return BadRequest("Neplatný uživatel");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GetUserOid for Username: {Username}, Email: {Email}", username, email);
                return new StatusCodeResult(500);
            }
            return Ok();
        }
    }
}
