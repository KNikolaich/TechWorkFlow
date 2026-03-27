using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Backend.Tests.Integration;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["Database:Provider"] = "PostgreSQL",
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Issuer"] = "TechWorkFlow",
                ["Jwt:Audience"] = "TechWorkFlow.Client",
                ["Jwt:SigningKey"] = "integration-tests-signing-key-minimum-32",
                ["Cors:AllowedOrigins:0"] = "http://localhost"
            };

            config.AddInMemoryCollection(values);
        });
    }
}
