using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;                // для .Where()
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Argus_WPF.Models;
using Argus_WPF.Windows;

namespace Argus_WPF.Pages
{
    public partial class TaskManagerPage : Page
    {
        private List<TaskItem> _tasks;

        private readonly string _taskFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.json"
        );

        public TaskManagerPage()
        {
            InitializeComponent();
            // Не вызываем LoadTasks тут, если рассчитываем на Page_Loaded
            // Но можно и здесь, лишь бы UpdateTasksCount() тоже было
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем задачи + обновляем счётчик, когда страница загружена
            LoadTasks();
            UpdateTasksCount();
        }

        private void LoadTasks()
        {
            try
            {
                if (!File.Exists(_taskFilePath))
                {
                    _tasks = new List<TaskItem>();
                }
                else
                {
                    string json = File.ReadAllText(_taskFilePath);
                    _tasks = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
                }
                TaskGrid.ItemsSource = _tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}");
                _tasks = new List<TaskItem>();
                TaskGrid.ItemsSource = _tasks;
            }
        }

        private void SaveTasks()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_tasks, options);
                File.WriteAllText(_taskFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения задач: {ex.Message}");
            }
        }

        private void RefreshGrid()
        {
            TaskGrid.ItemsSource = null;
            TaskGrid.ItemsSource = _tasks;
        }

        private void UpdateTasksCount()
        {
            // Допустим, у тебя есть TextBlock с x:Name="TasksCountLabel"
            TasksCountLabel.Text = $"Всего задач: {_tasks.Count}";
        }

        // Добавить задачу
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var newTask = new TaskItem
            {
                Executor = "",
                Description = "",
                Status = "Не начато",
                CreatedAt = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            var editWin = new EditTaskWindow(newTask);
            if (editWin.ShowDialog() == true)
            {
                _tasks.Add(newTask);
                RefreshGrid();
                SaveTasks();
                UpdateTasksCount();  // ВАЖНО: обновляем счётчик
            }
        }

        // Изменить задачу
        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskGrid.SelectedItem is TaskItem selectedTask)
            {
                var clone = new TaskItem
                {
                    Executor = selectedTask.Executor,
                    Description = selectedTask.Description,
                    Status = selectedTask.Status,
                    CreatedAt = selectedTask.CreatedAt,
                    LastUpdated = selectedTask.LastUpdated
                };

                var editWin = new EditTaskWindow(clone);
                if (editWin.ShowDialog() == true)
                {
                    selectedTask.Executor = clone.Executor;
                    selectedTask.Description = clone.Description;
                    selectedTask.Status = clone.Status;
                    selectedTask.LastUpdated = DateTime.Now;

                    RefreshGrid();
                    SaveTasks();
                    UpdateTasksCount();  // ВАЖНО
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для изменения");
            }
        }

        // Удалить задачу
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskGrid.SelectedItem is TaskItem selectedTask)
            {
                if (MessageBox.Show("Удалить задачу?", "Подтвердите",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _tasks.Remove(selectedTask);

                    RefreshGrid();
                    SaveTasks();
                    UpdateTasksCount(); // ВАЖНО
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для удаления");
            }
        }

        // Фильтр / поиск
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string executorSearch = txtSearchExecutor.Text.Trim().ToLower();
            string statusFilter = (cmbFilterStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var filtered = _tasks.Where(t =>
            {
                bool matchExec = string.IsNullOrEmpty(executorSearch)
                                 || t.Executor.ToLower().Contains(executorSearch);
                bool matchStatus = (statusFilter == "Все") || (t.Status == statusFilter);
                return matchExec && matchStatus;
            }).ToList();

            // Привязываем отфильтрованный список
            TaskGrid.ItemsSource = filtered;

            // Обновляем счётчик
            TasksCountLabel.Text = $"Всего задач: {filtered.Count}";
        }
    }
}
