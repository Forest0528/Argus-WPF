using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Argus_WPF.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAllAsync();
        Task SaveAllAsync(List<TaskItem> tasks);
    }
}
