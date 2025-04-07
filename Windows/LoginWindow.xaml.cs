using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Windows;
using Argus_WPF.Models;

namespace Argus_WPF
{
    public partial class LoginWindow : Window
    {
        private List<Employee> employees;
        public Employee LoggedInEmployee { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // Загружаем сотрудников из employees.json
            employees = LoadEmployeesFromJson();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string input = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            try
            {
                // Ищем сотрудника: либо по Id, либо по Name
                var user = employees.FirstOrDefault(emp =>
                    (emp.Id == input || emp.Name == input) && emp.Password == password);

                if (user != null)
                {
                    // Успешный логин
                    LoggedInEmployee = user;

                    // Открываем MainWindow
                    MainWindow mainWindow = new MainWindow(user);
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверные логин или пароль!", "Ошибка входа");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при входе: {ex.Message}", "Ошибка");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Отмена
        }

        private List<Employee> LoadEmployeesFromJson()
        {
            // employees.json в папке Data
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "employees.json"
            );
            if (!File.Exists(path)) return new List<Employee>();

            try
            {
                string json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<Employee>>(json);
                return list ?? new List<Employee>();
            }
            catch
            {
                return new List<Employee>();
            }
        }
    }
}
