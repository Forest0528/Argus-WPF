using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Argus_WPF.Models; // где у тебя класс Employee

namespace Argus_WPF.Pages
{
    public partial class EmployeePage : Page
    {
        public EmployeePage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "employees.json"
            );
            if (!File.Exists(path))
            {
                // Файл не найден — покажем пустую таблицу
                dataGridEmployees.ItemsSource = new List<Employee>();
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var employees = JsonSerializer.Deserialize<List<Employee>>(json)
                                ?? new List<Employee>();

                // Отображаем в таблице
                dataGridEmployees.ItemsSource = employees;
            }
            catch (Exception ex)
            {
                // Если ошибка чтения или десериализации
                dataGridEmployees.ItemsSource = new List<Employee>();
                System.Windows.MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }
    }
}
