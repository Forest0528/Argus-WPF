using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Argus_WPF.ViewModels
{
    /// <summary>
    /// ViewModel для управления списком задач:
    /// - хранит коллекцию Tasks
    /// - предоставляет фильтрацию по исполнителю и статусу
    /// - подсчитывает аналитику (количество задач разных статусов, топ исполнителей)
    /// - предоставляет команды (AddTaskCommand, DeleteTaskCommand)
    /// </summary>
    public partial class TaskViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;

        // Основные коллекции
        [ObservableProperty] private ObservableCollection<TaskItem> tasks = new();
        [ObservableProperty] private ObservableCollection<TaskItem> filteredTasks = new();

        // Выбранная задача (для, например, удаления или доп.информации)
        [ObservableProperty] private TaskItem? selectedTask;

        // Фильтры
        [ObservableProperty] private string filterStatus = "Все";
        [ObservableProperty] private string filterExecutor = string.Empty;

        // Общие счётчики
        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int notStartedCount;
        [ObservableProperty] private int inProgressCount;
        [ObservableProperty] private int completedCount;

        // Топ исполнителей (Dictionary: Имя исполнителя -> Количество задач)
        [ObservableProperty] private Dictionary<string, int> topExecutors = new();

        [ObservableProperty]
        private List<DailyAnalyticsItem> last7DaysAnalytics = new();

        /// <summary>
        /// Инициализирует сервис задач и загружает их из хранилища.
        /// </summary>
        public TaskViewModel(ITaskService taskService)
        {
            _taskService = taskService;
            _ = LoadTasksAsync();
        }

        /// <summary>
        /// Загружает все задачи из хранилища и обновляет UI.
        /// </summary>
        private async Task LoadTasksAsync()
        {
            var list = await _taskService.GetAllAsync();
            Tasks = new ObservableCollection<TaskItem>(list);
            ApplyFilters();
            UpdateAnalytics();
        }

        /// <summary>
        /// Команда добавления новой задачи.
        /// </summary>
        [RelayCommand]
        private async Task AddTask()
        {
            var newTask = new TaskItem
            {
                Executor = "",
                Description = "",
                Status = "Не начато",
                CreatedAt = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            var window = new Argus_WPF.Windows.EditTaskWindow(newTask);

            var result = window.ShowDialog();
            if (result == true)
            {
                Tasks.Add(newTask);
                ApplyFilters();
                UpdateAnalytics();
                await SaveTasksAsync();
            }
        }

        /// <summary>
        /// Команда удаления выбранной задачи.
        /// </summary>
        [RelayCommand]
        private async Task DeleteTask()
        {
            if (SelectedTask != null)
            {
                Tasks.Remove(SelectedTask);
                ApplyFilters();
                UpdateAnalytics();
                await SaveTasksAsync();
            }
        }

        // При изменении FilterStatus или FilterExecutor (или самой коллекции Tasks)
        partial void OnFilterStatusChanged(string value)
        {
            Console.WriteLine($"Filter changed to: {value}");
            ApplyFilters();
        }

        partial void OnFilterExecutorChanged(string value) => ApplyFilters();
        partial void OnTasksChanged(ObservableCollection<TaskItem> value)
        {
            ApplyFilters();
            UpdateAnalytics();
        }

        /// <summary>
        /// Применяем фильтры по статусу и исполнителю к списку задач.
        /// </summary>
        private void ApplyFilters()
        {
            var query = Tasks.AsEnumerable();

            // Фильтр по исполнителю (частичное совпадение)
            if (!string.IsNullOrWhiteSpace(FilterExecutor))
            {
                query = query.Where(t => t.Executor != null
                                         && t.Executor.IndexOf(FilterExecutor, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Фильтр по статусу
            if (!string.IsNullOrWhiteSpace(FilterStatus) && FilterStatus != "Все")
            {
                query = query.Where(t => string.Equals(t.Status, FilterStatus, StringComparison.OrdinalIgnoreCase));
            }

            FilteredTasks = new ObservableCollection<TaskItem>(query);
        }

        public List<string> Statuses { get; } = new()
        {
            "Все", "Не начато", "Выполняется", "Завершено"
        };


        /// <summary>
        /// Обновляет сводную аналитику: общее кол-во задач, распределение по статусам, топ исполнителей.
        /// </summary>
        private void UpdateAnalytics()
        {
            TotalTasks = Tasks.Count;
            NotStartedCount = Tasks.Count(t => t.Status == "Не начато");
            InProgressCount = Tasks.Count(t => t.Status == "Выполняется");
            CompletedCount = Tasks.Count(t => t.Status == "Завершено");

            TopExecutors = Tasks
                .Where(t => t.Status == "Завершено")
                .GroupBy(t => t.Executor)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            UpdateDailyAnalytics(); // <-- новая аналитика
        }

        private void UpdateDailyAnalytics()
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            var analytics = last7Days
                .Select(day => new DailyAnalyticsItem
                {
                    Date = day.ToString("dd.MM"),
                    Count = Tasks.Count(t => t.CreatedAt.Date == day.Date)
                })
                .ToList();

            Last7DaysAnalytics = analytics;
        }


        /// <summary>
        /// Сохраняет текущее состояние задач (для постоянного хранения).
        /// </summary>
        private async Task SaveTasksAsync()
        {
            await _taskService.SaveAllAsync(Tasks.ToList());
        }
    }
}
