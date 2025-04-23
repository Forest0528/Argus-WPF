using Argus_WPF.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus_WPF.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly string _filePath = Path.Combine("Data", "employees.json");

        public async Task<List<Employee>> GetAllAsync()
        {
            if (!File.Exists(_filePath)) return new List<Employee>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Employee>>(json) ?? new List<Employee>();
        }
    }
}
