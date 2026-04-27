using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.AiService.Settings;
using ToDoAI.Domain.Enums;
using ToDoAI.Infrastructure.Clients.AiService;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace ToDoAI.IntegrationTests.Clients;

public sealed class AiServiceClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    [Fact]
    public async Task GenerateSchedule_WhenAiServiceReturnsSuccess_ShouldReturnMappedPayload()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new AiGenerateScheduleRequest
        {
            ScheduleDate = new DateOnly(2026, 4, 27),
            DayStartAt = new DateTimeOffset(2026, 4, 27, 9, 0, 0, TimeSpan.Zero),
            DayEndAt = new DateTimeOffset(2026, 4, 27, 11, 0, 0, TimeSpan.Zero),
            Tasks =
            [
                new AiPlanningTaskRequest
                {
                    Id = taskId,
                    Title = "Focus task",
                    Description = "Finish the important thing",
                    EstimatedMinutes = 90,
                    RemainingMinutes = 60,
                    Priority = 5,
                    ComplexityLevel = 6,
                    DeadlineAt = new DateTimeOffset(2026, 4, 28, 18, 0, 0, TimeSpan.Zero),
                    WorkStatus = WorkStatus.New
                }
            ]
        };

        _server
            .Given(Request.Create().WithPath("/api/v1/ai/schedule/generate").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                          {
                            "scheduled": [
                              {
                                "taskId": "__TASK_ID__",
                                "title": "Focus task",
                                "startAt": "2026-04-27T09:00:00+00:00",
                                "endAt": "2026-04-27T10:00:00+00:00",
                                "plannedMinutes": 60,
                                "priority": 5,
                                "reasoning": "Высокий приоритет и свободное утро."
                              }
                            ],
                            "unscheduled": [],
                            "summary": {
                              "scheduleDate": "2026-04-27",
                              "availableMinutes": 120,
                              "plannedMinutes": 60,
                              "scheduledCount": 1,
                              "unscheduledCount": 0,
                              "explanations": [
                                "Сначала поставили самую важную задачу."
                              ],
                              "plannerModel": "google/gemini-2.5-flash",
                              "usedFallbackRanking": false,
                              "generatedAt": "2026-04-27T08:55:00+00:00"
                            }
                          }
                          """.Replace("__TASK_ID__", taskId.ToString())));

        var client = CreateScheduleClient();

        // Act
        var result = await client.GenerateSchedule(request, CancellationToken.None);

        // Assert
        result.UsedAi.Should().BeTrue();
        result.FallbackReason.Should().BeNull();
        result.Response.Should().NotBeNull();
        result.Response!.Scheduled.Should().ContainSingle();

        var block = result.Response.Scheduled.Single();
        block.TaskId.Should().Be(taskId);
        block.Title.Should().Be("Focus task");
        block.PlannedMinutes.Should().Be(60);
        block.Priority.Should().Be(5);
        block.Reasoning.Should().Be("Высокий приоритет и свободное утро.");

        result.Response.Summary.Should().NotBeNull();
        result.Response.Summary!.PlannerModel.Should().Be("google/gemini-2.5-flash");
        result.Response.Summary.Explanations.Should().ContainSingle("Сначала поставили самую важную задачу.");
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private AiServiceClient CreateScheduleClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_server.Urls[0])
        };

        return new AiServiceClient(
            httpClient,
            Options.Create(new AiServiceSettings
            {
                Enabled = true,
                BaseUrl = _server.Urls[0],
                GenerateSchedulePath = "/api/v1/ai/schedule/generate"
            }),
            NullLogger<AiServiceClient>.Instance);
    }
}
