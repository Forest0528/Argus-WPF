using Argus_WPF.Models;
using Argus_WPF.Pages;
using Argus_WPF.UserControls;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Argus_WPF
{
    public partial class MainWindow : Window
    {
        private readonly Employee currentUser;

        public MainWindow(Employee emp)
        {
            InitializeComponent();
            currentUser = emp;

            Title = $"Argus — {currentUser.Name} ({currentUser.Role})";
            PageTitle.Text = "Главная";
            MainFrame.Navigate(new DashboardPage(currentUser));
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "Главная";
            MainFrame.Navigate(new DashboardPage(currentUser));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FooterName.Text = currentUser.Name;
            FooterRole.Text = currentUser.Role;

            if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
            {
                try
                {
                    FooterAvatar.Fill = new ImageBrush(new BitmapImage(new Uri(currentUser.AvatarUrl)))
                    {
                        Stretch = Stretch.UniformToFill
                    };
                }
                catch
                {
                    FooterAvatar.Fill = new SolidColorBrush(Colors.Gray);
                }
            }
            else
            {
                FooterAvatar.Fill = new SolidColorBrush(Colors.Gray);
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

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TaskManagerPage());
            PageTitle.Text = "Проекты";
        }

    }
}
