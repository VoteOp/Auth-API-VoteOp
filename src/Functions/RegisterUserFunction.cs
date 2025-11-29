using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using VoteOp.AuthApi.Data;
using VoteOp.AuthApi.Models;
using VoteOp.AuthApi.Security;
using VoteOp.AuthApi.Utils;

namespace VoteOp.AuthApi.Functions;

public class RegisterUserFunction
{
    private readonly ILogger _logger;
    private readonly UserRepository _userRepo;

    public RegisterUserFunction(ILoggerFactory loggerFactory, UserRepository userRepo)
    {
        _logger = loggerFactory.CreateLogger<RegisterUserFunction>();
        _userRepo = userRepo;
    }

    [Function("RegisterUser")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")]
        HttpRequestData req)
    {
        var response = req.CreateResponse();

        var model = await req.ReadFromJsonAsync<RegisterRequest>();
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            await response.WriteStringAsync("Email and password are required.");
            return response;
        }

        // basic validation
        if (model.Password.Length < 8)
        {
            response.StatusCode = HttpStatusCode.BadRequest;
            await response.WriteStringAsync("Password must be at least 8 characters.");
            return response;
        }

        // check if user exists
        var existing = await _userRepo.GetByEmailAsync(model.Email);
        if (existing != null)
        {
            response.StatusCode = HttpStatusCode.Conflict;
            await response.WriteStringAsync("Email already registered.");
            return response;
        }

        var (hash, salt) = PasswordHasher.HashPassword(model.Password);

        var entity = new UserEntity
        {
            Email = model.Email.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        await _userRepo.CreateAsync(entity);

        response.StatusCode = HttpStatusCode.Created;
        await response.WriteStringAsync("User registered.");
        return response;
    }
}