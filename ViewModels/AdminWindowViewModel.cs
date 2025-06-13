using Argus_WPF.Helpers;
using Argus_WPF.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Argus_WPF.ViewModels
{
    public class AdminEmployeeViewModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsRoleEditable { get; set; }
    }

    public class AdminWindowViewModel : INotifyPropertyChanged
    {
        private readonly string _employeeFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");

        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery == value) return;
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                CollectionViewSource.GetDefaultView(Employees).Refresh();
            }
        }

        public ObservableCollection<AdminEmployeeViewModel> Employees { get; }
            = new ObservableCollection<AdminEmployeeViewModel>();

        private AdminEmployeeViewModel? _selectedEmployee;
        public AdminEmployeeViewModel? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public AdminWindowViewModel()
        {
            // Настраиваем команды
            AddEmployeeCommand = new RelayCommand(_ => AddEmployee());
            DeleteEmployeeCommand = new RelayCommand(
                p => DeleteEmployee(p as AdminEmployeeViewModel),
                p => p is AdminEmployeeViewModel);

            // Загружаем список
            LoadEmployees();

            // Настраиваем фильтр
            var view = CollectionViewSource.GetDefaultView(Employees);
            view.Filter = FilterEmployees;
        }

        private bool FilterEmployees(object o)
        {
            if (o is not AdminEmployeeViewModel emp) return false;
            if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
            var q = SearchQuery.Trim().ToLowerInvariant();
            return emp.Name.ToLowerInvariant().Contains(q)
                || emp.Role.ToLowerInvariant().Contains(q);
        }

        private void AddEmployee()
        {
            var dialog = new Windows.AddEmployeeDialog { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;

            var newEmp = dialog.NewEmployee;
            // Загружаем исходный список моделей
            var list = ReadRawEmployees();
            if (list.Any(e => e.Id == newEmp.Id || e.Login == newEmp.Login))
            {
                MessageBox.Show("Сотрудник с таким ID или логином уже существует.",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            list.Add(newEmp);
            File.WriteAllText(_employeeFile, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
            LoadEmployees();
        }

        private void DeleteEmployee(AdminEmployeeViewModel? vm)
        {
            if (vm == null) return;
            if (MessageBox.Show($"Удалить сотрудника «{vm.Name}»?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Question)
                                != MessageBoxResult.Yes) return;

            var list = ReadRawEmployees();
            var toRemove = list.FirstOrDefault(e => e.Id == vm.Id);
            if (toRemove != null)
            {
                list.Remove(toRemove);
                File.WriteAllText(_employeeFile, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
                LoadEmployees();
            }
        }

        private void LoadEmployees()
        {
            Employees.Clear();
            var raw = ReadRawEmployees();
            var currentUser = App.CurrentUser;
            foreach (var emp in raw)
            {
                Employees.Add(new AdminEmployeeViewModel
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    Role = emp.Role,
                    IsRoleEditable = currentUser != null &&
                                     (currentUser.Role == "Директор" || currentUser.Role == "Руководитель")
                });
            }
        }

        private List<Employee> ReadRawEmployees()
        {
            if (!File.Exists(_employeeFile))
                return new List<Employee>();

            try
            {
                var json = File.ReadAllText(_employeeFile);
                return JsonSerializer.Deserialize<List<Employee>>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<Employee>();
            }
            catch
            {
                return new List<Employee>();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
