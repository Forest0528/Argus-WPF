using Argus_WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Argus_WPF.Pages
{
    public partial class TaskManagerPage : Page
    {
        public TaskManagerPage()
        {
            InitializeComponent();
            // Привязываем DataContext к нужной VM (через DI)
            DataContext = App.AppHost.Services.GetRequiredService<TaskViewModel>();
        }
    }
}
