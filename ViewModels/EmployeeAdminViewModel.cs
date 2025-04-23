using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Argus_WPF.Models;

namespace Argus_WPF.ViewModels
{
    public class EmployeeAdminViewModel : INotifyPropertyChanged
    {
        public EmployeeAdminViewModel(Employee employee, bool canEdit)
        {
            Model = employee;
            IsRoleEditable = canEdit;

            AvailableRoles = new List<string>
            {
                "Директор", "Руководитель", "Тимлид",
                "Разработчик", "Веб-разработчик", "Дизайнер", "SMM", "QA | Backend"
            };
        }

        public Employee Model { get; }

        public string Id => Model.Id;
        public string Name => Model.Name;

        public string Role
        {
            get => Model.Role;
            set
            {
                if (Model.Role != value)
                {
                    Model.Role = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRoleEditable { get; set; }

        public List<string> AvailableRoles { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
