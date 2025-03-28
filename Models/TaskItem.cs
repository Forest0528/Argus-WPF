using System;

public class TaskItem
{
    public string Executor { get; set; } // Исполнитель
    public string Description { get; set; } // Описание задачи
    public string Status { get; set; } // Не начато / Выполняется / Завершено
    public DateTime CreatedAt { get; set; } // Дата создания
    public DateTime LastUpdated { get; set; } // Последнее обновление
}
