using Argus_WPF.Models;
using Argus_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Argus_WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly IEmployeeService _employeeService;
        private readonly Employee _currentUser;

        [ObservableProperty]
        private ObservableCollection<Employee> employees = new();

        [ObservableProperty]
        private ObservableCollection<TimeRecord> timeRecords = new();

        private static GoogleSheetsSettings _googleSheetsSettings;
        private SheetsService _sheetsService;
        private System.Threading.Timer _syncTimer;

        public DashboardViewModel(IEmployeeService employeeService, Employee currentUser)
        {
            _employeeService = employeeService;
            _currentUser = currentUser;

            LoadAppSettings();

            if (_googleSheetsSettings != null)
            {
                InitializeGoogleSheetsService();
                _ = LoadDataFromGoogleSheetAsync(); // Первоначальная загрузка

                // Запуск таймера: первая сработка через 1 минуту, затем каждые 5 минут
                _syncTimer = new System.Threading.Timer(async _ => await LoadDataFromGoogleSheetAsync(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));
            }
            // else: Настройки не загружены, сообщение об ошибке уже было показано в LoadAppSettings
        }

        private static void LoadAppSettings()
        {
            if (_googleSheetsSettings == null)
            {
                System.Diagnostics.Debug.WriteLine("Attempting to load appsettings.json...");
                try
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    var settings = new GoogleSheetsSettings();
                    configuration.GetSection("GoogleSheets").Bind(settings);

                    if (string.IsNullOrEmpty(settings.CredentialsPath) ||
                        string.IsNullOrEmpty(settings.SpreadsheetId) ||
                        string.IsNullOrEmpty(settings.SheetName) ||
                        string.IsNullOrEmpty(settings.ApplicationName))
                    {
                        MessageBox.Show("Конфигурация Google Sheets (ApplicationName, CredentialsPath, SpreadsheetId, SheetName) не найдена или неполна в appsettings.json.",
                                        "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
                        System.Diagnostics.Debug.WriteLine("Google Sheets configuration is missing or incomplete in appsettings.json.");
                        _googleSheetsSettings = null;
                        return;
                    }
                    _googleSheetsSettings = settings;
                    System.Diagnostics.Debug.WriteLine("Application settings loaded successfully.");
                    System.Diagnostics.Debug.WriteLine($"CredentialsPath: '{_googleSheetsSettings.CredentialsPath}', SpreadsheetId: '{_googleSheetsSettings.SpreadsheetId}', SheetName: '{_googleSheetsSettings.SheetName}'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке appsettings.json: {ex.Message}",
                                   "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine($"Error loading appsettings.json: {ex.Message}");
                    _googleSheetsSettings = null;
                }
            }
        }

        private void InitializeGoogleSheetsService()
        {
            if (_googleSheetsSettings == null || string.IsNullOrEmpty(_googleSheetsSettings.CredentialsPath))
            {
                System.Diagnostics.Debug.WriteLine("Google Sheets settings not loaded or CredentialsPath is empty. Skipping service initialization.");
                return;
            }
            System.Diagnostics.Debug.WriteLine("Initializing Google Sheets service...");
            try
            {
                GoogleCredential credential;
                string fullCredentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _googleSheetsSettings.CredentialsPath);

                if (!File.Exists(fullCredentialsPath))
                {
                    MessageBox.Show($"Файл учетных данных Google Sheets не найден: {fullCredentialsPath}\nПроверьте путь в appsettings.json (GoogleSheets:CredentialsPath).", "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Diagnostics.Debug.WriteLine($"Google Sheets credentials file not found: {fullCredentialsPath}");
                    return;
                }

                using (var stream = new FileStream(fullCredentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
                }

                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _googleSheetsSettings.ApplicationName,
                });
                System.Diagnostics.Debug.WriteLine("Google Sheets service initialized successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации сервиса Google Sheets: {ex.Message}\nУбедитесь, что API Google Sheets включен и файл ключа корректен.", "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"Error initializing Google Sheets service: {ex.Message}");
                _sheetsService = null;
            }
        }

        private async Task LoadDataFromGoogleSheetAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadDataFromGoogleSheetAsync called.");

            if (_googleSheetsSettings == null)
            {
                System.Diagnostics.Debug.WriteLine("Google Sheets settings are null. Aborting data load.");
                return;
            }

            if (_sheetsService == null)
            {
                System.Diagnostics.Debug.WriteLine("Google Sheets service is null. Attempting re-initialization...");
                InitializeGoogleSheetsService();

                if (_sheetsService == null)
                {
                    System.Diagnostics.Debug.WriteLine("Google Sheets service still null after re-initialization. Aborting data load.");
                    return;
                }
            }

            string range = $"{_googleSheetsSettings.SheetName}!A2:D";
            System.Diagnostics.Debug.WriteLine($"Attempting to load data from Google Sheet. SpreadsheetId: '{_googleSheetsSettings.SpreadsheetId}', Range: '{range}'");

            try
            {
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        _sheetsService.Spreadsheets.Values.Get(_googleSheetsSettings.SpreadsheetId, range);

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                var newRecords = new List<TimeRecord>();

                if (values != null && values.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Received {values.Count} rows from Google Sheet.");
                    foreach (var row in values)
                    {
                        if (row.Count < 4)
                        {
                            System.Diagnostics.Debug.WriteLine($"Skipping row with insufficient columns: {string.Join(", ", row.Select(c => c?.ToString() ?? "NULL"))}");
                            continue;
                        }

                        string employeeName = row[0]?.ToString()?.Trim();
                        string arrivalTimeString = row[1]?.ToString()?.Trim();
                        string departureTimeString = row[2]?.ToString()?.Trim();
                        string dateString = row[3]?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(employeeName) || string.IsNullOrEmpty(dateString))
                        {
                            System.Diagnostics.Debug.WriteLine($"Skipping row due to empty employee name or date. Employee: '{employeeName}', Date: '{dateString}'");
                            continue;
                        }

                        if (!DateTime.TryParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime recordDate))
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse date: '{dateString}' for employee '{employeeName}'. Skipping record.");
                            continue;
                        }

                        bool recordAddedThisRow = false;
                        if (!string.IsNullOrEmpty(arrivalTimeString))
                        {
                            // Пытаемся распознать время как "h:mm" (например, "9:30") или "hh:mm" (например, "09:30")
                            if (TimeSpan.TryParseExact(arrivalTimeString, "h\\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan arrivalTime) ||
                                TimeSpan.TryParseExact(arrivalTimeString, "hh\\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out arrivalTime))
                            {
                                newRecords.Add(new TimeRecord
                                {
                                    EmployeeLogin = employeeName,
                                    Timestamp = recordDate.Add(arrivalTime),
                                    Action = "Пришёл"
                                });
                                recordAddedThisRow = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to parse arrival time: '{arrivalTimeString}' for employee '{employeeName}' on date '{dateString}'.");
                            }
                        }

                        if (!string.IsNullOrEmpty(departureTimeString))
                        {
                            if (TimeSpan.TryParseExact(departureTimeString, "h\\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan departureTime) ||
                                TimeSpan.TryParseExact(departureTimeString, "hh\\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out departureTime))
                            {
                                newRecords.Add(new TimeRecord
                                {
                                    EmployeeLogin = employeeName,
                                    Timestamp = recordDate.Add(departureTime),
                                    Action = "Ушёл"
                                });
                                recordAddedThisRow = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to parse departure time: '{departureTimeString}' for employee '{employeeName}' on date '{dateString}'.");
                            }
                        }
                        if (recordAddedThisRow)
                        {
                            System.Diagnostics.Debug.WriteLine($"Successfully processed records for '{employeeName}' on '{dateString}'. Arrival: '{arrivalTimeString}', Departure: '{departureTimeString}'");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No data (rows) received from Google Sheet or 'values' collection is null/empty.");
                }

                System.Diagnostics.Debug.WriteLine($"Prepared {newRecords.Count} TimeRecord objects to display.");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TimeRecords = new ObservableCollection<TimeRecord>(newRecords.OrderBy(r => r.Timestamp));
                    System.Diagnostics.Debug.WriteLine($"TimeRecords collection updated on UI thread with {TimeRecords.Count} items.");
                });
            }
            catch (Google.GoogleApiException apiEx) // Более специфичный перехват ошибок Google API
            {
                System.Diagnostics.Debug.WriteLine($"Google API Error loading data from Google Sheets: {apiEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Google API Error Details: {(apiEx.Error != null ? string.Join("; ", apiEx.Error.Errors.Select(e => $"Reason: {e.Reason}, Message: {e.Message}")) : "No details")}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Ошибка API Google Sheets: {apiEx.Message}\nПроверьте права доступа сервисного аккаунта к таблице, правильность Spreadsheet ID и включен ли Google Sheets API для вашего проекта.", "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Generic error loading data from Google Sheets: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        /*
        [RelayCommand]
        private async Task Arrived()
        {
            MessageBox.Show("Функция 'Пришёл' должна быть адаптирована для работы с Google Sheets.", "Информация");
        }

        [RelayCommand]
        private async Task Left()
        {
            MessageBox.Show("Функция 'Ушёл' должна быть адаптирована для работы с Google Sheets.", "Информация");
        }
        */

        public void Dispose()
        {
            _syncTimer?.Dispose();
            System.Diagnostics.Debug.WriteLine("DashboardViewModel disposed, sync timer stopped.");
        }
    }
}