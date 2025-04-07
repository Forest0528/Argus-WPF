using System.Configuration;
using System.Data;
using System.Windows;

namespace Argus_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var login = new LoginWindow();
            bool? result = login.ShowDialog();
            // Показываем окно логина модально

            if (result == false && login.LoggedInEmployee != null)
            {
                // Успешно вошли
                var mainWin = new MainWindow(login.LoggedInEmployee);
                mainWin.Show();
            }
            else
            {
                // Либо отмена, либо неверный логин - выходим
                Shutdown();
            }
        }

    }

}
