using System.ComponentModel;

namespace ToDoAI.Domain.Enums;

public enum ErrorCodes
{
    [Description("Unauthorized request")]
    NotAuthorized = 0,

    [Description("User with this username already exists")]
    UserExists = 1,

    [Description("User with this username does not exist")]
    UserDoesNotExist = 2,

    [Description("Task not found")]
    TaskNotFound = 3,

    [Description("User state not found")]
    UserStateNotFound = 4,

    [Description("Incorrect parameter value")]
    IncorrectValue = 5,

    [Description("Invalid task status transition")]
    InvalidTaskStatusTransition = 6,

    [Description("Task execution feedback already exists")]
    TaskExecutionAlreadyExists = 7,

    [Description("Task should be completed")]
    TaskShouldBeCompleted = 8,

    [Description("Schedule not found for task")]
    ScheduleNotFound = 10,

    [Description("Schedule does not match task")]
    ScheduleDoesNotMatchTask = 11,

    [Description("Day schedule not found")]
    DayScheduleNotFound = 12,

    [Description("User cannot have multiple open work sessions")]
    TaskWorkSessionAlreadyExists = 13,

    [Description("Open session not found")]
    SessionNotFound = 14,

    [Description("Invalid task work session status")]
    InvalidTaskWorkSessionStatus = 15,

    [Description("Email is not confirmed")]
    EmailNotConfirmed = 16,

    [Description("AI service is unavailable")]
    AiServiceUnavailable = 17,

    [Description("AI service request timed out")]
    AiServiceTimeout = 18,

    [Description("AI service returned an invalid response")]
    AiServiceInvalidResponse = 19,
}
