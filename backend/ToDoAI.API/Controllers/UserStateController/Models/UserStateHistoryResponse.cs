namespace ToDoAI.API.Controllers.UserStateController.Models;

public sealed class UserStateHistoryResponse
{
    public IReadOnlyCollection<UserStateResponse> History { get; init; } = [];
}