using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using System.Collections.ObjectModel;
using System.Windows;


namespace Argus_WPF.ViewModels
{
    public partial class ThemeManagerViewModel : ObservableObject
    {
        public ObservableCollection<string> Themes { get; } = new()
        {
            "Light.Blue", "Light.Green", "Light.Red", "Light.Orange", "Light.Steel",
            "Dark.Blue", "Dark.Green", "Dark.Red", "Dark.Orange", "Dark.Steel"
        };

        [ObservableProperty]
        private string selectedTheme;

        public ThemeManagerViewModel()
        {
            SelectedTheme = Properties.Settings.Default.Theme;
            Console.WriteLine("Theme from settings: " + SelectedTheme);

            if (string.IsNullOrWhiteSpace(SelectedTheme) || !Themes.Contains(SelectedTheme))
            {
                Console.WriteLine("Invalid theme found. Resetting to Light.Blue");
                SelectedTheme = "Light.Blue";
            }

            ApplyTheme();
        }

        [RelayCommand]
        private void ApplyTheme()
        {
            if (!Themes.Contains(SelectedTheme))
            {
                SelectedTheme = "Light.Blue";
            }

            ThemeManager.Current.ChangeTheme(Application.Current, SelectedTheme);
            Properties.Settings.Default.Theme = SelectedTheme;
            Properties.Settings.Default.Save();
        }

    }
}
