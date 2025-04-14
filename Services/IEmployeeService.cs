using Argus_WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Argus_WPF.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(string id);
        Task SaveAsync(List<Employee> employees);
    }
}
