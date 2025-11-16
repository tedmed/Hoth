using MessagingContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace WeatherEye.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        public AuthController(ILogger<AuthController> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            

            using IServiceScope scope = serviceScopeFactory.CreateScope();

            var bus = scope.ServiceProvider.GetRequiredService<IMessageContext>();

            var res = await bus.InvokeAsync<UserLoginResponse>(request);

            if (!res.IsSuccessful)
            {
                return Unauthorized(new { IsSuccessful = false, Token = string.Empty });
            }

            return Ok(new { IsSuccessful = true, Token = res.Token });
        }

       
    }
}
