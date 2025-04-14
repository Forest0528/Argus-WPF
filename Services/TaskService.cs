using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus_WPF.Services
{
    public class TaskService : ITaskService
    {
        private readonly string _path = Path.Combine("Data", "tasks.json");

        public async Task<List<TaskItem>> GetAllAsync()
        {
            if (!File.Exists(_path)) return new();

            try
            {
                var json = await File.ReadAllTextAsync(_path);
                var list = JsonSerializer.Deserialize<List<TaskItem>>(json);
                return list ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task SaveAllAsync(List<TaskItem> tasks)
        {
            try
            {
                var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_path, json);
            }
            catch
            {
                // handle error
            }
        }
    }
}
