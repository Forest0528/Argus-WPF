using System;
using System.Windows;
using Argus_WPF.Properties;
using Argus_WPF.Services;
using Argus_WPF.ViewModels;
using Argus_WPF.Views;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Argus_WPF
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Сервисы
                    services.AddSingleton<IEmployeeService, EmployeeService>();
                    services.AddSingleton<ITaskService, TaskService>(); // 🟢 Добавить вот это!

                    // ViewModels
                    services.AddSingleton<TaskViewModel>();
                    services.AddSingleton<ThemeManagerViewModel>();
                    services.AddSingleton<DashboardViewModel>();

                    // Окна
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<LoginWindow>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Устанавливаем тему через ThemeManager
            ThemeManager.Current.ChangeTheme(this, Settings.Default.Theme);

            var login = AppHost.Services.GetRequiredService<LoginWindow>();
            login.Show();
        }
    }
}