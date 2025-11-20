using MessagingContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
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
        public IActionResult GetUserOid()
        {
            _logger.LogInformation("UserController GetUserOid method called");

            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader?.StartsWith("Bearer ") != true)
                return BadRequest("No token");

            var token = authHeader.Substring("Bearer ".Length);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // příklad: získání claimů
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Missing username or email in token claims. Username: {Username}, Email: {Email}", username, email);
                return BadRequest("Missing username or email in token claims.");
            }

            using IServiceScope scope = serviceScopeFactory.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();
            try
            {
                var response = bus.InvokeAsync<UserOidResponse>(new MessagingContracts.UserOidRequest(username, email)).Result;
                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing GetUserOid for Username: {Username}, Email: {Email}", username, email);
                return new StatusCodeResult(500);
            }
        }
    }
}
