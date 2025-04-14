using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Argus_WPF.Models;
using Argus_WPF.Helpers;
using Argus_WPF.Services;
using Argus_WPF.Views;

// Google OAuth
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Util.Store;
using Google.Apis.Util;
using Google.Apis.Services;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;

namespace Argus_WPF
{
    public partial class LoginWindow : Window
    {
        private readonly IEmployeeService _employeeService;
        private List<Employee> employees;
        public Employee LoggedInEmployee { get; private set; }

        public LoginWindow(IEmployeeService employeeService)
        {
            InitializeComponent();
            _employeeService = employeeService;
            this.Loaded += (s, e) => txtLogin.Focus();
            employees = LoadEmployeesFromJson();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string input = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            try
            {
                var user = employees.FirstOrDefault(emp =>
                    (emp.Id == input || emp.Name == input) && emp.Password == password);

                if (user != null)
                {
                    LoggedInEmployee = user;

                    MainWindow mainWindow = new MainWindow(user, _employeeService);
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

        private async void btnGoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tokenPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "ArgusWPF.GoogleOAuth");

                if (Directory.Exists(tokenPath))
                    Directory.Delete(tokenPath, true);

                string clientId = AppConfig.GoogleClientId;
                string clientSecret = AppConfig.GoogleClientSecret;
                var scopes = new[] { "openid", "email", "profile" };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },
                    Scopes = scopes,
                    DataStore = new FileDataStore("ArgusWPF.GoogleOAuth", true)
                });

                var codeReceiver = new LocalServerCodeReceiver();
                var app = new AuthorizationCodeInstalledApp(flow, codeReceiver);
                var credential = await app.AuthorizeAsync("user", CancellationToken.None);

                if (credential.Token.IsExpired(SystemClock.Default))
                    await credential.RefreshTokenAsync(CancellationToken.None);

                string accessToken = credential.Token.AccessToken;
                var userInfo = await GoogleOAuthHelper.GetUserInfoAsync(accessToken);

                var googleUser = new Employee
                {
                    Id = userInfo.Id,
                    Name = userInfo.Name ?? userInfo.Email,
                    Password = "N/A",
                    Role = "User",
                    AvatarUrl = userInfo.Picture
                };

                var mainWindow = new MainWindow(googleUser, _employeeService);
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа через Google: {ex.Message}", "OAuth Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private List<Employee> LoadEmployeesFromJson()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "employees.json");
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

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Здесь будет логика восстановления пароля…");
        }

        private void txtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtPassword.Focus();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnLogin_Click(btnLogin, null);
        }
    }
}