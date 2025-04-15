using Argus_WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Argus_WPF.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Если создаёте VM вручную:
            this.DataContext = new ThemeManagerViewModel();

            // Если используете DI (вроде Microsoft.Extensions.DependencyInjection):
            // this.DataContext = App.AppHost.Services.GetRequiredService<ThemeManagerViewModel>();
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем значения настроек уведомлений
            CheckEmailNotify.IsChecked = Properties.Settings.Default.NotifyEmail;
            CheckSoundNotify.IsChecked = Properties.Settings.Default.NotifySound;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Сохраняем состояние настроек уведомлений
            Properties.Settings.Default.NotifyEmail = CheckEmailNotify.IsChecked == true;
            Properties.Settings.Default.NotifySound = CheckSoundNotify.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс настроек уведомлений к состоянию по умолчанию
            CheckEmailNotify.IsChecked = false;
            CheckSoundNotify.IsChecked = false;

            Properties.Settings.Default.NotifyEmail = false;
            Properties.Settings.Default.NotifySound = false;
            Properties.Settings.Default.Save();
        }
    }
}
