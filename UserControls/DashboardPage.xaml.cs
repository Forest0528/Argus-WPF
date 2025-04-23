using Argus_WPF.Models;
using Argus_WPF.Services;
using Argus_WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Argus_WPF.UserControls
{
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(Employee user)
        {
            InitializeComponent();

            // Предположим, у вас есть DI-контейнер, но мы вручную:
            // var employeeService = new SomeEmployeeService();
            // _viewModel = new DashboardViewModel(employeeService, user);

            // Или, если используете AppHost:
            var employeeService = App.AppHost.Services.GetService(typeof(IEmployeeService)) as IEmployeeService;
            _viewModel = new DashboardViewModel(employeeService, user);

            this.DataContext = _viewModel;

            GreetUser(user);
        }

        private void GreetUser(Employee currentUser)
        {
            GreetingTextBlock.Text = $"Добро пожаловать, {currentUser.Name} ({currentUser.Role})!";

            if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
            {
                try
                {
                    AvatarImage.Visibility = Visibility.Visible;
                    AvatarImage.Source = new BitmapImage(new Uri(currentUser.AvatarUrl));
                }
                catch
                {
                    AvatarImage.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
