using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows; // для MessageBox, если нужно

namespace Argus_WPF.ViewModels
{
    /// <summary>
    /// ViewModel для управления списком задач:
    /// - хранит коллекцию Tasks
    /// - предоставляет фильтрацию по исполнителю и статусу
    /// - подсчитывает аналитику (количество задач разных статусов, топ исполнителей)
    /// - команды (AddTask, DeleteTask) + "Пришёл"/"Ушёл" (запись в data.json)
    /// </summary>
    public partial class TaskViewModel : ObservableObject
    {
        // 1) Сервис для задач
        private readonly ITaskService _taskService;

        // 2) Путь к data.json (где храним "Пришёл"/"Ушёл")
        private readonly string _timeRecordFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "data.json");

        // 3) Текущий пользователь (логин, роль) - берём из App.Current.Properties
        private readonly Employee _currentUser;

        // -------------------
        // Коллекции для задач
        // -------------------
        [ObservableProperty] private ObservableCollection<TaskItem> tasks = new();
        [ObservableProperty] private ObservableCollection<TaskItem> filteredTasks = new();

        // Выбранная задача
        [ObservableProperty] private TaskItem? selectedTask;

        // Фильтры
        [ObservableProperty] private string filterStatus = "Все";
        [ObservableProperty] private string filterExecutor = string.Empty;

        // Общие счётчики
        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int notStartedCount;
        [ObservableProperty] private int inProgressCount;
        [ObservableProperty] private int completedCount;

        // Топ исполнителей
        [ObservableProperty] private Dictionary<string, int> topExecutors = new();

        [ObservableProperty]
        private List<DailyAnalyticsItem> last7DaysAnalytics = new();

        /// <summary>
        /// Конструктор: теперь **только** ITaskService, 
        /// а текущего юзера берём через App.Current.Properties["CurrentUser"].
        /// </summary>
        public TaskViewModel(ITaskService taskService)
        {
            _taskService = taskService;

            // Пытаемся взять текущего пользователя из App.Current.Properties
            // Убедитесь, что вы установили App.Current.Properties["CurrentUser"] где-то при логине
            _currentUser = App.Current.Properties["CurrentUser"] as Employee;
            if (_currentUser == null)
            {
                // Например, если не нашли, можно создать «заглушку» или вывести сообщение
                _currentUser = new Employee
                {
                    Login = "Unknown",
                    Role = "User"
                };
            }

            _ = LoadTasksAsync();
        }

        // =============================
        //  КОМАНДЫ УПРАВЛЕНИЯ ЗАДАЧАМИ
        // =============================

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

        // =============================
        //  "ПРИШЁЛ" / "УШЁЛ"
        // =============================

        [RelayCommand]
        private async Task Arrived()
        {
            await RecordTimeAsync("Пришёл");
        }

        [RelayCommand]
        private async Task Left()
        {
            await RecordTimeAsync("Ушёл");
        }

        /// <summary>
        /// Записывает в data.json (TimeRecord) событие "Пришёл"/"Ушёл" для _currentUser.
        /// </summary>
        private async Task RecordTimeAsync(string action)
        {
            try
            {
                // 1. Считываем текущие записи
                List<TimeRecord> records = new();
                if (File.Exists(_timeRecordFile))
                {
                    var existingJson = await File.ReadAllTextAsync(_timeRecordFile);
                    records = JsonSerializer.Deserialize<List<TimeRecord>>(existingJson) ?? new List<TimeRecord>();
                }

                // 2. Добавляем новую запись
                records.Add(new TimeRecord
                {
                    EmployeeLogin = _currentUser.Login,
                    Timestamp = DateTime.Now,
                    Action = action
                });

                // 3. Сохраняем
                var updatedJson = JsonSerializer.Serialize(records, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_timeRecordFile, updatedJson);
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                System.Windows.MessageBox.Show($"Ошибка записи в data.json: {ex.Message}");
            }
        }

        // ===================================
        //  ЗАГРУЗКА/СОХРАНЕНИЕ СПИСКА ЗАДАЧ
        // ===================================

        private async Task LoadTasksAsync()
        {
            var list = await _taskService.GetAllAsync();
            Tasks = new ObservableCollection<TaskItem>(list);

            ApplyFilters();
            UpdateAnalytics();
        }

        private async Task SaveTasksAsync()
        {
            await _taskService.SaveAllAsync(Tasks.ToList());
        }

        // ===================================
        //  ФИЛЬТРЫ
        // ===================================

        public List<string> Statuses { get; } = new()
        {
            "Все", "Не начато", "Выполняется", "Завершено"
        };

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

        // ===================================
        //  АНАЛИТИКА
        // ===================================

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

            UpdateDailyAnalytics();
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
    }
}
