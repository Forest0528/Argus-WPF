using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Argus_WPF.Models;

// Google OAuth
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Util.Store;
using Google.Apis.Util;
using Google.Apis.Services;
using Google.Apis.Oauth2.v2;          // библиотека для userinfo
using Google.Apis.Oauth2.v2.Data; // где объявлен класс Userinfoplus


namespace Argus_WPF
{
    public partial class LoginWindow : Window
    {
        private List<Employee> employees;
        public Employee LoggedInEmployee { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => txtLogin.Focus();
            employees = LoadEmployeesFromJson();
        }

        // Обычный вход по логину/паролю
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

        // Войти через Google
        private async void btnGoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    // 🧹 Удаляем сохранённые токены (иначе Google не предложит выбрать аккаунт)
            //    string tokenPath = Path.Combine(
            //        Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            //        "ArgusWPF.GoogleOAuth");

            //    if (Directory.Exists(tokenPath))
            //        Directory.Delete(tokenPath, true);

            //    // 📌 Настройка OAuth 2.0
            //    var clientId = "";
            //    var clientSecret = "";
            //    var scopes = new[] { "openid", "email", "profile" };

            //    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            //    {
            //        ClientSecrets = new ClientSecrets
            //        {
            //            ClientId = clientId,
            //            ClientSecret = clientSecret
            //        },
            //        Scopes = scopes,
            //        DataStore = new FileDataStore("ArgusWPF.GoogleOAuth", true) // путь уже удалили выше
            //    });

            //    var codeReceiver = new LocalServerCodeReceiver();
            //    var app = new AuthorizationCodeInstalledApp(flow, codeReceiver);
            //    var credential = await app.AuthorizeAsync("user", CancellationToken.None);

            //    if (credential.Token.IsExpired(SystemClock.Default))
            //        await credential.RefreshTokenAsync(CancellationToken.None);

            //    string accessToken = credential.Token.AccessToken;

            //    // 📥 Получаем информацию о пользователе
            //    var userInfo = await GoogleOAuthHelper.GetUserInfoAsync(accessToken);

            //    // 📄 Регистрируем или заходим под сотрудником
            //    var googleUser = new Employee
            //    {
            //        Id = userInfo.Id,
            //        Name = userInfo.Name ?? userInfo.Email,
            //        Password = "N/A",
            //        Role = "User",
            //        AvatarUrl = userInfo.Picture
            //    };

            //    // ✅ Запускаем основное окно
            //    var mainWindow = new MainWindow(googleUser);
            //    Application.Current.MainWindow = mainWindow;
            //    mainWindow.Show();

            //    this.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка входа через Google: {ex.Message}", "OAuth Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private List<Employee> LoadEmployeesFromJson()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "employees.json");
            if (!File.Exists(path))
                return new List<Employee>();

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
            {
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin_Click(btnLogin, null);
            }
        }
    }
}
