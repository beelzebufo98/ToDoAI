using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.AiService.Settings;

namespace ToDoAI.Infrastructure.Clients.AiService;

public sealed class AiServiceClient : IAiScheduleClient
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
    private readonly ILogger<AiServiceClient> _logger;

    public AiServiceClient(
        HttpClient httpClient,
        IOptions<AiServiceSettings> settings,
        ILogger<AiServiceClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiGenerateScheduleResult> GenerateSchedule(
        AiGenerateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            return new AiGenerateScheduleResult
            {
                UsedAi = false,
                FallbackReason = "disabled"
            };
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                _settings.GenerateSchedulePath,
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI service returned non-success status code {StatusCode} for schedule generation.",
                    (int)response.StatusCode);
                return new AiGenerateScheduleResult
                {
                    UsedAi = false,
                    FallbackReason = "http_error"
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<AiGenerateScheduleResponse>(
                JsonOptions,
                cancellationToken);
            if (payload is null)
            {
                return new AiGenerateScheduleResult
                {
                    UsedAi = false,
                    FallbackReason = "invalid_json"
                };
            }

            return new AiGenerateScheduleResult
            {
                UsedAi = true,
                Response = payload
            };
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "AI service request failed. Falling back to deterministic schedule generation.");
            return new AiGenerateScheduleResult
            {
                UsedAi = false,
                FallbackReason = "http_error"
            };
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "AI service request timed out. Falling back to deterministic schedule generation.");
            return new AiGenerateScheduleResult
            {
                UsedAi = false,
                FallbackReason = "timeout"
            };
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "AI service returned invalid JSON. Falling back to deterministic schedule generation.");
            return new AiGenerateScheduleResult
            {
                UsedAi = false,
                FallbackReason = "invalid_json"
            };
        }
    }
}
