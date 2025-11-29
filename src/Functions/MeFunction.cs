using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace VoteOp.AuthApi.Functions;

public class MeFunction
{
    private readonly IConfiguration _config;
    private readonly ILogger<MeFunction> _logger;

    public MeFunction(IConfiguration config, ILogger<MeFunction> logger)
    {
        _config = config;
        _logger = logger;
    }

    [Function("Me")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/me")] HttpRequestData req)
    {
        string? authHeader = req.Headers.GetValues("Authorization").FirstOrDefault();

        if (authHeader is null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var unauthorized = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Missing or invalid Authorization header.");
            return unauthorized;
        }

        string token = authHeader["Bearer ".Length..].Trim();

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler
            {
                // <<< IMPORTANT: keep claim types as-is (sub, email, etc.)
                MapInboundClaims = false
            };

            var keyBytes = Encoding.UTF8.GetBytes(_config["JwtKey"]!);
            var key = new SymmetricSecurityKey(keyBytes);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _config["JwtIssuer"],
                ValidAudience = _config["JwtAudience"],
                IssuerSigningKey = key
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken _);

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                userId,
                email
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed.");
            var unauthorized = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Invalid or expired token.");
            return unauthorized;
        }
    }
}