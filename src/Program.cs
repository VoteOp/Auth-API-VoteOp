using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using VoteOp.AuthApi.Data;
using VoteOp.AuthApi.Security;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // DI: config is already registered by the host
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<UserRepository>();
        services.AddSingleton<JwtTokenGenerator>();
    })
    .Build();

host.Run();