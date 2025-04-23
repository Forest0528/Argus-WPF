using Argus_WPF.Models;  // где объявлен класс Employee
using MahApps.Metro.Controls;
using System.Windows;

namespace Argus_WPF.Windows
{
    public partial class AddEmployeeDialog : MetroWindow
    {
        // Сюда диалог положит готового сотрудника
        public Employee NewEmployee { get; private set; }

        public AddEmployeeDialog()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // 1. Проверяем, все ли поля заполнены
            if (string.IsNullOrWhiteSpace(IdBox.Text) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(MailBox.Text))
            {
                MessageBox.Show("Заполните все поля перед добавлением.",
                                "Проверка полей",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // 2. Определяем выбранную роль
            var selectedRoleItem = RoleBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
            string selectedRole = selectedRoleItem?.Content?.ToString() ?? "Разработчик";

            // 3. Создаём объект сотрудника
            NewEmployee = new Employee
            {
                Id = IdBox.Text.Trim(),
                Name = NameBox.Text.Trim(),
                Login = LoginBox.Text.Trim(),
                Password = PasswordBox.Password,
                Email = MailBox.Text.Trim(),
                Role = selectedRole
            };

            // 4. Закрываем окно (передаём успех)
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
