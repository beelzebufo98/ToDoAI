using System.ComponentModel;

namespace ToDoAI.ToDoAI.Domain.Enums;

public enum ErrorCodes
{
    [Description("Не авторизованный запрос")]
    NotAuthorized = 0,
    
    [Description("Пользователь с таким никнеймом уже существует")]
    UserExists = 1,
    
    [Description("Пользователь с таким никнеймом не существует")]
    UserDoesNotExist = 2,
    
    [Description("Задача отсутствует")]
    TaskNotFound = 3,
    
    [Description("Нет задач по текущему пользователю")]
    TasksNotFound = 4,
}