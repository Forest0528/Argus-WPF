using Argus_WPF.Models;
using Argus_WPF.Pages;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Argus_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Employee currentUser;

        public MainWindow(Employee emp)
        {
            InitializeComponent();
            currentUser = emp;
            Title = $"Argus — {currentUser.Name} ({currentUser.Role})";
            MainFrame.Navigate(new EmployeePage());
            //// Получаем байты из Resources
            //byte[] logoBytes = Properties.Resources.Argus_logo;

            //// Конвертим в BitmapImage
            //var bmp = ConvertBytesToImage(logoBytes);

            //// Присваиваем
            //LogoImage.Source = bmp;
        }

        private void OpenEmployeePage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EmployeePage());
        }

        private void OpenTaskManagerPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TaskManagerPage());
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
            PageTitle.Text = "Главная";
        }

        private BitmapImage ConvertBytesToImage(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze(); // чтобы использовать из разных потоков
                return bmp;
            }
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TaskManagerPage());
            NavigateTo(new TaskManagerPage(), "Задачи", sender as Button);
        }

        private void NavigateTo(Page page, string title, Button selectedButton)
        {
            MainFrame.Navigate(page);
            PageTitle.Text = title;

            // Подсветка активной кнопки
            ResetMenuButtonsStyle();
            selectedButton.Background = new SolidColorBrush(Color.FromRgb(220, 230, 250)); // светлый оттенок
        }

        private void ResetMenuButtonsStyle()
        {
            foreach (var child in MenuPanel.Children)
            {
                if (child is Button btn)
                    btn.ClearValue(Button.BackgroundProperty);
            }
        }

    }
}