using System;
using System.Windows;
using Argus_WPF.Properties;

namespace Argus_WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var login = new LoginWindow();
            login.Show();
        }
    }
}
