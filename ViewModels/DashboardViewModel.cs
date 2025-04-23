using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Windows; // для MessageBox

namespace Argus_WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;

        // Храним текущего пользователя, чтобы знать, чей логин писать
        private readonly Employee _currentUser;

        // Коллекция сотрудников (если нужно)
        [ObservableProperty]
        private ObservableCollection<Employee> employees = new();

        // Коллекция записей (событий) для DataGrid
        [ObservableProperty]
        private ObservableCollection<TimeRecord> timeRecords = new();

        // Путь к data.json (можно скорректировать под себя)
        private readonly string dataPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "data.json");

        // Конструктор. Передаём сервис + текущего юзера
        public DashboardViewModel(IEmployeeService employeeService, Employee currentUser)
        {
            _employeeService = employeeService;
            _currentUser = currentUser;

            _ = LoadDataAsync();
        }

        /// <summary>
        /// Основной метод загрузки: читаем сотрудников (по необходимости),
        /// читаем data.json и кладём в TimeRecords.
        /// </summary>
        private async Task LoadDataAsync()
        {
            // Если нужно — подгружаем сотрудников
            var list = await _employeeService.GetAllAsync();
            Employees = new ObservableCollection<Employee>(list);

            // Если файла нет — пропускаем
            if (!File.Exists(dataPath))
                return;

            try
            {
                string json = await File.ReadAllTextAsync(dataPath);
                var records = JsonSerializer.Deserialize<List<TimeRecord>>(json);
                TimeRecords = new ObservableCollection<TimeRecord>(records ?? new List<TimeRecord>());
            }
            catch
            {
                // Лог или игнор
            }
        }

        // ===========================
        // КОМАНДЫ "ПРИШЁЛ" / "УШЁЛ"
        // ===========================

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
        /// Пишем в data.json новую строку: (EmployeeLogin, Timestamp=Now, Action=?)
        /// Но перед этим проверяем логику:
        ///  - "Пришёл" дважды подряд нельзя
        ///  - "Ушёл" без "Пришёл" нельзя
        /// Затем заново грузим TimeRecords, чтобы таблица обновилась.
        /// </summary>
        private async Task RecordTimeAsync(string action)
        {
            // 1. Читаем текущие записи
            var records = new List<TimeRecord>();
            if (File.Exists(dataPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(dataPath);
                    records = JsonSerializer.Deserialize<List<TimeRecord>>(json) ?? new List<TimeRecord>();
                }
                catch
                {
                    // игнорируем / логируем
                }
            }

            // 2. Проверяем логику прихода/ухода
            // Находим последнюю запись для этого пользователя (если есть)
            var lastRecordForUser = records
                .Where(r => r.EmployeeLogin == _currentUser.Login)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault();

            if (action == "Пришёл")
            {
                // Нельзя "Пришёл", если последняя запись тоже "Пришёл" (и нет "Ушёл")
                if (lastRecordForUser != null && lastRecordForUser.Action == "Пришёл")
                {
                    MessageBox.Show("Вы уже отметили «Пришёл». Сначала нажмите «Ушёл», прежде чем снова приходить.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }
            }
            else if (action == "Ушёл")
            {
                // Нельзя "Ушёл", если не было прихода или последняя запись уже "Ушёл"
                if (lastRecordForUser == null)
                {
                    MessageBox.Show("Невозможно «Уйти», не приходя.",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }
                if (lastRecordForUser.Action == "Ушёл")
                {
                    MessageBox.Show("Вы уже отметили «Ушёл». Сначала нажмите «Пришёл».",
                                    "Внимание",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }
            }

            // 3. Добавляем новую запись
            records.Add(new TimeRecord
            {
                EmployeeLogin = _currentUser.Login, // или Name, как хотите
                Timestamp = DateTime.Now,
                Action = action
            });

            // 4. Сохраняем обратно
            try
            {
                var updatedJson = JsonSerializer.Serialize(records, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(dataPath, updatedJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

            // 5. Перечитываем, чтобы DataGrid обновился
            TimeRecords = new ObservableCollection<TimeRecord>(records);
        }
    }
}
