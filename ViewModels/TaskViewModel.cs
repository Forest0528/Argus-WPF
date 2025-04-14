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
    public partial class TaskViewModel : ObservableObject
    {
        private readonly ITaskService _taskService;

        [ObservableProperty] private ObservableCollection<TaskItem> tasks = new();
        [ObservableProperty] private ObservableCollection<TaskItem> filteredTasks = new();
        [ObservableProperty] private TaskItem? selectedTask;
        [ObservableProperty] private string filterStatus = "Все";
        [ObservableProperty] private string filterExecutor = string.Empty;
        [ObservableProperty] private int totalTasks;
        [ObservableProperty] private int notStartedCount;
        [ObservableProperty] private int inProgressCount;
        [ObservableProperty] private int completedCount;

        // УДАЛЯЕМ все, что связано с графиком:
        // [ObservableProperty] private Dictionary<string, int> tasksByDate = new();
        // [ObservableProperty] private PlotModel taskPlotModel = new();

        [ObservableProperty] private Dictionary<string, int> topExecutors = new();

        public TaskViewModel(ITaskService taskService)
        {
            _taskService = taskService;
            _ = LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            var list = await _taskService.GetAllAsync();
            Tasks = new ObservableCollection<TaskItem>(list);
            ApplyFilters();
            UpdateAnalytics();
        }

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

            Tasks.Add(newTask);
            ApplyFilters();
            UpdateAnalytics();
            await SaveTasksAsync();
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

        partial void OnFilterStatusChanged(string value) => ApplyFilters();
        partial void OnFilterExecutorChanged(string value) => ApplyFilters();
        partial void OnTasksChanged(ObservableCollection<TaskItem> value)
        {
            ApplyFilters();
            UpdateAnalytics();
        }

        private void ApplyFilters()
        {
            var query = Tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterExecutor))
                query = query.Where(t => t.Executor?.ToLower().Contains(FilterExecutor.ToLower()) == true);

            if (FilterStatus != "Все")
                query = query.Where(t => t.Status == FilterStatus);

            FilteredTasks = new ObservableCollection<TaskItem>(query);
        }

        private void UpdateAnalytics()
        {
            TotalTasks = Tasks.Count;
            NotStartedCount = Tasks.Count(t => t.Status == "Не начато");
            InProgressCount = Tasks.Count(t => t.Status == "Выполняется");
            CompletedCount = Tasks.Count(t => t.Status == "Завершено");

            // TasksByDate = Tasks
            //     .GroupBy(t => t.CreatedAt.ToString("dd.MM"))
            //     .ToDictionary(g => g.Key, g => g.Count());

            TopExecutors = Tasks
                .Where(t => t.Status == "Завершено")
                .GroupBy(t => t.Executor)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Count());

            // GenerateChart();
        }

        private async Task SaveTasksAsync()
        {
            await _taskService.SaveAllAsync(Tasks.ToList());
        }
    }
}
