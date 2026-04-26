using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.AiService.Settings;

namespace ToDoAI.Infrastructure.Clients.AiService;

public sealed class AiMotivationClient : IAiMotivationClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    private readonly HttpClient _httpClient;
    private readonly AiServiceSettings _settings;
    private readonly ILogger<AiMotivationClient> _logger;

    public AiMotivationClient(
        HttpClient httpClient,
        IOptions<AiServiceSettings> settings,
        ILogger<AiMotivationClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiGenerateMotivationResult> GenerateMotivation(
        AiGenerateMotivationRequest request,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "disabled"
            };
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                _settings.GenerateMotivationPath,
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI service returned non-success status code {StatusCode} for motivation generation.",
                    (int)response.StatusCode);
                return new AiGenerateMotivationResult
                {
                    UsedAi = false,
                    FallbackReason = "http_error"
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<AiGenerateMotivationResponse>(
                JsonOptions,
                cancellationToken);
            if (payload is null)
            {
                return new AiGenerateMotivationResult
                {
                    UsedAi = false,
                    FallbackReason = "invalid_json"
                };
            }

            return new AiGenerateMotivationResult
            {
                UsedAi = true,
                Response = payload
            };
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "AI motivation request failed. Falling back to deterministic message.");
            return new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "http_error"
            };
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "AI motivation request timed out. Falling back to deterministic message.");
            return new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "timeout"
            };
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "AI motivation response contained invalid JSON. Falling back to deterministic message.");
            return new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "invalid_json"
            };
        }
    }
}