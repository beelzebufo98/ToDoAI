using System.ComponentModel;

namespace ToDoAI.Domain.Enums;

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
    
    [Description("Нет состояний по текущему пользователю")]
    UserStateNotFound = 4,
    
    [Description("Некорретное значение параметров")]
    IncorrectValue = 5,

    [Description("Некорретное изменение статуса задачи")]
    InvalidTaskStatusTransition = 6,
    
    [Description("Обратная связь по задаче уже существует")]
    TaskExecutionAlreadyExists = 7,
    
    [Description("Задача должна быть выполнена")]
    TaskShouldBeCompleted = 8,
    
    [Description("Расписание для задачи не найдено")]
    ScheduleNotFound = 10,

    [Description("Расписание не связано с задачей")]
    ScheduleDoesNotMatchTask = 11,
    
    [Description("Расписание на указанную дату не найдено")]
    DayScheduleNotFound = 12,
    
    [Description("У пользователя не может быть несколько открытых сессий")]
    TaskWorkSessionAlreadyExists = 13,
    
    [Description("Открытая сессия отсутствует")]
    SessionNotFound = 14,
    
    [Description("Invalid task work session status")]
    InvalidTaskWorkSessionStatus = 15,

    [Description("Email пользователя не подтвержден")]
    EmailNotConfirmed = 16,
}
