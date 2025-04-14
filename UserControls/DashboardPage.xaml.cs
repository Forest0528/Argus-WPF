using Argus_WPF.Models;
using Argus_WPF.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Argus_WPF.UserControls
{
    public partial class DashboardPage : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(Employee user)
        {
            InitializeComponent();

            _viewModel = App.AppHost.Services.GetService(typeof(DashboardViewModel)) as DashboardViewModel;
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
                    AvatarImage.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
