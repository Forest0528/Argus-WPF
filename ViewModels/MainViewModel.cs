using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Argus_WPF.Helpers;


namespace Argus_WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public string WelcomeText => $"Добро пожаловать, {Environment.UserName}!";

        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Refresh());
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void Refresh()
        {
            MessageBox.Show("Тут будет обновление задач");
        }

        private void Logout()
        {
            MessageBox.Show("Выход из системы...");
            Application.Current.Shutdown(); // позже заменим на реальный logout
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
