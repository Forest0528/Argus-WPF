using Argus_Project;
using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus_WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;

        [ObservableProperty]
        private ObservableCollection<Employee> employees = new();

        [ObservableProperty]
        private ObservableCollection<TimeRecord> timeRecords = new();

        public DashboardViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var list = await _employeeService.GetAllAsync();
            Employees = new ObservableCollection<Employee>(list);

            var dataPath = Path.Combine("Data", "data.json");
            if (File.Exists(dataPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(dataPath);
                    var records = JsonSerializer.Deserialize<List<TimeRecord>>(json);
                    var latestByEmployee = records?
                        .GroupBy(r => r.EmployeeName)
                        .Select(g => g.OrderByDescending(r => r.ArrivalTime).First())
                    .ToList();

                    TimeRecords = new ObservableCollection<TimeRecord>(latestByEmployee);
                }
                catch { }
            }
        }
    }
}