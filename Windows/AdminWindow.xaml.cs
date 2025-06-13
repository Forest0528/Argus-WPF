using System.Windows;
using MahApps.Metro.Controls;
using Argus_WPF.ViewModels;

namespace Argus_WPF.Windows
{
    public partial class AdminWindow : MetroWindow
    {
        public AdminWindow()
        {
            InitializeComponent();
            DataContext = new AdminWindowViewModel();
        }
    }
}
