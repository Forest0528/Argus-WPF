using Argus_WPF.Models;
using Argus_WPF.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Argus_WPF.Windows
{
    public partial class AdminWindow : MetroWindow
    {
        private List<Employee> _employees = new();
        private readonly string employeeFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");

        public AdminWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        // Нажали "Добавить сотрудника"
        private void AddEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            // 1) Показываем диалог
            var dialog = new AddEmployeeDialog
            {
                Owner = this
            };

            // 2) Если пользователь нажал "Добавить" внутри диалога
            if (dialog.ShowDialog() == true)
            {
                var newEmp = dialog.NewEmployee;

                // 3) Проверяем дубликаты
                if (_employees.Any(emp => emp.Id == newEmp.Id || emp.Login == newEmp.Login))
                {
                    MessageBox.Show("Сотрудник с таким ID или логином уже существует.");
                    return;
                }

                // 4) Добавляем в список
                _employees.Add(newEmp);

                // 5) Сохраняем
                SaveEmployees();

                // 6) Обновляем DataGrid
                LoadEmployees();
            }
        }

        // Загрузка
        private void LoadEmployees()
        {
            if (!File.Exists(employeeFile))
            {
                _employees = new List<Employee>();
                return;
            }

            try
            {
                var json = File.ReadAllText(employeeFile);
                _employees = JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();

                // Какая-то логика прав на редактирование (пример)
                var currentUser = App.Current.Properties["CurrentUser"] as Employee;
                var viewModels = _employees.Select(emp => new AdminEmployeeViewModel
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    Role = emp.Role,
                    IsRoleEditable = currentUser != null &&
                                     (currentUser.Role == "Директор" || currentUser.Role == "Руководитель")
                }).ToList();

                // Привязываем в DataGrid
                AdminEmployeeGrid.ItemsSource = viewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        // Сохранение
        private void SaveEmployees()
        {
            try
            {
                var json = JsonSerializer.Serialize(_employees, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(employeeFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения сотрудников: {ex.Message}");
            }
        }
    }
}
