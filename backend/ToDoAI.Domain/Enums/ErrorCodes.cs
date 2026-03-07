using System.ComponentModel;

namespace ToDoAI.ToDoAI.Domain;

public enum ErrorCodes
{
    [Description("Не авторизованный запрос")]
    NotAuthorized = 0
}