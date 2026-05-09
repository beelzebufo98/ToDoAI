using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.AiService.Settings;

namespace ToDoAI.Infrastructure.Clients.AiService;

public sealed class AiTaskAssistantClient : IAiTaskAssistantClient
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
    private readonly ILogger<AiTaskAssistantClient> _logger;
    
    public AiTaskAssistantClient(
        HttpClient httpClient,
        IOptions<AiServiceSettings> settings,
        ILogger<AiTaskAssistantClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AiAssistTaskResult> GetAssistTask(
        AiAssistTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            throw new InvalidOperationException("AI task assistance is disabled.");
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                _settings.GenerateTaskAssistPath,
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI service returned non-success status code {StatusCode} for task assistance.",
                    (int)response.StatusCode);
                throw new HttpRequestException($"AI service returned status code {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<AiAssistTaskResult>(JsonOptions, cancellationToken);
            if (payload is null)
            {
                throw new JsonException("AI task assistance response payload is empty.");
            }

            return payload;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "AI task assistance request failed.");
            throw;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "AI task assistance request timed out.");
            throw;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "AI task assistance response contained invalid JSON.");
            throw;
        }
    }
}