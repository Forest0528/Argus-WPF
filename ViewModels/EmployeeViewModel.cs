using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Argus_WPF.ViewModels
{
    public partial class EmployeeViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;

        [ObservableProperty]
        private ObservableCollection<Employee> employees;

        public EmployeeViewModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
            Employees = new ObservableCollection<Employee>();
            LoadEmployeesCommand = new AsyncRelayCommand(LoadEmployees);
        }

        public IAsyncRelayCommand LoadEmployeesCommand { get; }

        private async Task LoadEmployees()
        {
            var data = await _employeeService.GetAllAsync();
            Employees = new ObservableCollection<Employee>(data);
        }
    }
}
