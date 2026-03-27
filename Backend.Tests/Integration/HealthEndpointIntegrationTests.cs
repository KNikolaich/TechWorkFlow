using FluentAssertions;

namespace Backend.Tests.Integration;

public sealed class HealthEndpointIntegrationTests : IClassFixture<PostgreSqlContainerFixture>
{
    private readonly PostgreSqlContainerFixture _fixture;

    public HealthEndpointIntegrationTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_Should_Return_Ok()
    {
        if (!_fixture.IsAvailable)
        {
            return;
        }

        await using var factory = new CustomWebApplicationFactory(_fixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
