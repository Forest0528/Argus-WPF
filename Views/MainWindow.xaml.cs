using Argus_WPF.Models;
using Argus_WPF.Pages;
using Argus_WPF.Services;
using Argus_WPF.UserControls;
using Argus_WPF.Windows;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Argus_WPF.Views
{
    public partial class MainWindow : MetroWindow
    {
        private readonly Employee _currentUser;
        private readonly IEmployeeService _employeeService;

        public MainWindow(Employee currentUser, IEmployeeService employeeService)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _employeeService = employeeService;
            this.Loaded += MainWindow_Loaded;

            Title = $"Argus — {_currentUser.Name} ({_currentUser.Role})";
            PageTitle.Text = "Главная";
            MainFrame.Navigate(new DashboardPage(_currentUser));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FooterName.Text = _currentUser.Name;
            FooterRole.Text = _currentUser.Role;

            try
            {
                if (!string.IsNullOrWhiteSpace(_currentUser.AvatarUrl))
                {
                    FooterAvatar.Fill = new ImageBrush(new BitmapImage(new Uri(_currentUser.AvatarUrl)))
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
                else
                {
                    FooterAvatar.Fill = new SolidColorBrush(Colors.Gray);
                }
            }
            catch
            {
                FooterAvatar.Fill = new SolidColorBrush(Colors.Gray);
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Главная";
            MainFrame.Navigate(new DashboardPage(_currentUser));
        }

        private void OpenEmployeePage(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Сотрудники";
            MainFrame.Navigate(new EmployeePage());
        }

        private void OpenTaskManagerPage(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Задачи";
            MainFrame.Navigate(new TaskManagerPage());
        }

        private void OpenSettingsPage(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Настройки";
            MainFrame.Navigate(new SettingsPage());
        }

        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Проекты";
            MainFrame.Navigate(new TaskManagerPage());
        }
        private void Admins_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.Role == "Директор" || _currentUser.Role == "Руководитель")
            {
                var adminWindow = new AdminWindow();
                adminWindow.Show();
            }
            else
            {
                MessageBox.Show("У вас нет прав доступа к этой странице.", "Отказано", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tokenPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".credentials",
                    "ArgusWPF.GoogleOAuth");

                if (Directory.Exists(tokenPath))
                {
                    Directory.Delete(tokenPath, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Logout Error");
            }

            var loginWindow = App.AppHost.Services.GetRequiredService<LoginWindow>();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            this.Close();
        }
    }
}