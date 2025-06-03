using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Argus_WPF.Models;
using Argus_WPF.Services;
using Argus_WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Argus_WPF.UserControls
{
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(Employee currentUser)
        {
            InitializeComponent();

            var employeeService = ResolveEmployeeService();
            _viewModel = new DashboardViewModel(employeeService, currentUser);
            DataContext = _viewModel;

            GreetUser(currentUser);
        }

        #region Helpers

        private static IEmployeeService ResolveEmployeeService()
        {
            var service = App.AppHost.Services.GetService<IEmployeeService>();
            if (service is null)
                throw new InvalidOperationException("IEmployeeService не зарегистрирован в DI-контейнере.");

            return service;
        }

        private void GreetUser(Employee user)
        {
            GreetingTextBlock.Text = $"Добро пожаловать, {user.Name} ({user.Role})!";

            if (string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                AvatarEllipse.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                AvatarImageBrush.ImageSource = new BitmapImage(new Uri(user.AvatarUrl, UriKind.Absolute));
                AvatarEllipse.Visibility = Visibility.Visible;
            }
            catch (UriFormatException) { AvatarEllipse.Visibility = Visibility.Collapsed; }
            catch (System.IO.IOException) { AvatarEllipse.Visibility = Visibility.Collapsed; }
            catch (NotSupportedException) { AvatarEllipse.Visibility = Visibility.Collapsed; }
        }

        #endregion
    }
}
