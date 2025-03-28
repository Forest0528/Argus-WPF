using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Argus_WPF.Models;
using System.Text.Json;
using System.IO;

namespace Argus_WPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        // Уже есть SeriesCollection, Labels...
        public int TotalEmployees { get; set; }

        public DashboardPage()
        {
            InitializeComponent();

            // 1) Читаем сотрудников (ниже метод)
            LoadEmployees();

            // 2) Пример данных для графика
            SeriesCollection = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Пример",
                Values = new ChartValues<double> {3, 5, 7, 4, 5, 6}
            },
            new ColumnSeries
            {
                Title = "Колонки",
                Values = new ChartValues<double> {1, 3, 2, 4, 7, 3}
            }
        };

            Labels = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн" };

            DataContext = this; // теперь XAML может показывать TotalEmployees
        }

        private void LoadEmployees()
        {
            // Путь к файлу employees.json (допустим, лежит в папке Data)
            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, // базовая папка
                "Data",
                "employees.json"
            );

            // Если файла нет — 0 сотрудников
            if (!File.Exists(path))
            {
                TotalEmployees = 0;
                return;
            }

            try
            {
                // Читаем JSON
                string json = File.ReadAllText(path);
                var employees = JsonSerializer.Deserialize<List<Employee>>(json);

                if (employees != null)
                    TotalEmployees = employees.Count;
                else
                    TotalEmployees = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка чтения employees.json: " + ex.Message);
                TotalEmployees = 0;
            }
        }
    }
}
