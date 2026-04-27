using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.AiService.Settings;
using ToDoAI.Infrastructure.Clients.AiService;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ToDoAI.IntegrationTests.Clients;

public sealed class AiMotivationClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    [Fact]
    public async Task GenerateMotivation_WhenAiServiceReturnsInvalidJson_ShouldReturnInvalidJsonFallback()
    {
        // Arrange
        _server
            .Given(Request.Create().WithPath("/api/v1/ai/motivation/generate").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{ invalid json }"));

        var client = CreateMotivationClient();

        // Act
        var result = await client.GenerateMotivation(
            new AiGenerateMotivationRequest
            {
                Trigger = "login"
            },
            CancellationToken.None);

        // Assert
        result.UsedAi.Should().BeFalse();
        result.Response.Should().BeNull();
        result.FallbackReason.Should().Be("invalid_json");
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private AiMotivationClient CreateMotivationClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_server.Urls[0])
        };

        return new AiMotivationClient(
            httpClient,
            Options.Create(new AiServiceSettings
            {
                Enabled = true,
                BaseUrl = _server.Urls[0],
                GenerateMotivationPath = "/api/v1/ai/motivation/generate"
            }),
            NullLogger<AiMotivationClient>.Instance);
    }
}
