using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VoteOp.AuthApi.Data;
using VoteOp.AuthApi.Models;
using VoteOp.AuthApi.Security;
using VoteOp.AuthApi.Utils;

namespace VoteOp.AuthApi.Functions;

public class LoginUserFunction
{
    private readonly ILogger _logger;
    private readonly UserRepository _userRepo;
    private readonly JwtTokenGenerator _jwt;

    public LoginUserFunction(ILoggerFactory loggerFactory, UserRepository userRepo, JwtTokenGenerator jwt)
    {
        _logger = loggerFactory.CreateLogger<LoginUserFunction>();
        _userRepo = userRepo;
        _jwt = jwt;
    }

    [Function("LoginUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")]
        HttpRequestData req)
    {
        var response = req.CreateResponse();

        var model = await req.ReadFromJsonAsync<LoginRequest>();
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            await response.WriteStringAsync("Email and password are required.");
            return response;
        }

        var user = await _userRepo.GetByEmailAsync(model.Email);
        if (user == null || !PasswordHasher.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt))
        {
            response.StatusCode = HttpStatusCode.Unauthorized;
            await response.WriteStringAsync("Invalid credentials.");
            return response;
        }

        var token = _jwt.Generate(user.Id, user.Email);

        response.StatusCode = HttpStatusCode.OK;
        await response.WriteAsJsonAsync(new AuthResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful."
        });

        return response;
    }
}