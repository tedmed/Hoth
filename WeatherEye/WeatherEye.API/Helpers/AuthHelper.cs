using System.IdentityModel.Tokens.Jwt;

namespace WeatherEye.API.Helpers
{
    public static class AuthHelper
    {
        public static void GetParametersFromAuthHeader(HttpRequest request, out string? email, out string? username)
        {
            var authHeader = request.Headers["Authorization"].ToString();

            if (authHeader?.StartsWith("Bearer ") != true)
                throw new UnauthorizedAccessException("Invalid Authorization header.");

            var token = authHeader.Substring("Bearer ".Length);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // příklad: získání claimů
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            username = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        }
    }
}
