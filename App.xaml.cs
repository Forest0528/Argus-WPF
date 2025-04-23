using Argus_WPF.Services;
using Argus_WPF.ViewModels;
using Argus_WPF.Views;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Argus_WPF
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }
        public static Models.Employee CurrentUser { get; set; }


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

            ThemeManager.Current.ChangeTheme(
            this,
            global::Argus_WPF.Properties.Settings.Default.Theme
            );
            var login = AppHost.Services.GetRequiredService<LoginWindow>();
            login.Show();
        }
    }
}