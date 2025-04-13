using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Argus_WPF.Models;
using System.Windows.Media.Imaging; // для BitmapImage
using System.Windows;
using Argus_Project;              // для Visibility

namespace Argus_WPF.UserControls
{
    public partial class DashboardPage : UserControl
    {
        private Employee currentUser;

        public DashboardPage(Employee user)
        {
            InitializeComponent();
            currentUser = user;
            LoadTimeRecords();
            GreetUser();
        }

        private void GreetUser()
        {
            // Пишем имя и роль
            GreetingTextBlock.Text = $"Добро пожаловать, {currentUser.Name} ({currentUser.Role})!";

            // Пробуем загрузить аватар
            if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
            {
                try
                {
                    AvatarImage.Visibility = Visibility.Visible;
                    AvatarImage.Source = new BitmapImage(new Uri(currentUser.AvatarUrl));
                }
                catch
                {
                    // Если не получилось загрузить, оставим скрытым
                    AvatarImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadTimeRecords()
        {
            try
            {
                string jsonPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data",
                    "data.json");

                if (!File.Exists(jsonPath)) return;

                string json = File.ReadAllText(jsonPath);
                var records = System.Text.Json.JsonSerializer
                               .Deserialize<List<TimeRecord>>(json);

                // Группируем по последнему визиту каждого сотрудника
                var latestByEmployee = records?
                    .GroupBy(r => r.EmployeeName)
                    .Select(g => g.OrderByDescending(r => r.ArrivalTime).First())
                    .ToList();

                EmployeeGrid.ItemsSource = latestByEmployee;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }
    }
}