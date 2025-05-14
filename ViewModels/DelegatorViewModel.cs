using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Argus_WPF.Services;

namespace Argus_WPF.ViewModels;

/// <summary>VM для страницы DelegatorPage.xaml</summary>
public partial class DelegatorViewModel : ObservableObject
{
    private readonly DelegatorApiClient _api;
    private const string CsrfToken =
        "8RJ8nH9T1IZX03KEE7mjMgliLkM9gqaSqajysjWiL5mBucXkvJuoR4b0mFPzH7IX";

    public DelegatorViewModel() : this(new DelegatorApiClient()) { }
    public DelegatorViewModel(DelegatorApiClient api) => _api = api;

    // ────────── чекбоксы-интеграции ──────────
    [ObservableProperty] private bool isGoogleCalendarEnabled;
    [ObservableProperty] private bool isGmailEnabled;
    [ObservableProperty] private bool isGoogleSheetsEnabled;
    [ObservableProperty] private bool isTrelloEnabled;
    [ObservableProperty] private bool isNotionEnabled;
    [ObservableProperty] private bool isAsanaEnabled;

    // ────────── параметры запроса ──────────
    [ObservableProperty] private string? email = "developer@vintorum.com";
    [ObservableProperty] private DateTime startDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime endDate = DateTime.Today;

    // ────────── коллекции для UI ──────────
    public ObservableCollection<string> RepeatingTasks { get; } = new();
    public ObservableCollection<TaskClassification> Classifications { get; } = new();

    // ────────── команды ──────────

    /// <summary>Запрашивает анализ на сервере и заполняет коллекции</summary>
    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
            return;

        try
        {
            var body = new DelegatorApiClient.AnalyzeRequest(
                source: "gmail",
                email: Email!,
                filters: new { }          // пустой объект
            );

            var resp = await _api.AnalyzeAsync(body, CsrfToken);
            if (resp is null) return;

            // --- повторяющиеся задачи ---
            RepeatingTasks.Clear();
            foreach (var t in resp.repeating_tasks)
                RepeatingTasks.Add(t);

            // --- классификация ---
            Classifications.Clear();
            foreach (var c in resp.classifications)
            {
                Classifications.Add(new TaskClassification
                {
                    Task = c.task,
                    Type = c.type,
                    Hours = c.hours
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            // TODO: Snackbar / MessageBox
        }
    }

    /// <summary>Пример заглушки импорта (остался из начального ТЗ)</summary>
    [RelayCommand]
    private void ImportData()
    {
        RepeatingTasks.Clear();
        RepeatingTasks.Add("Задача-пример 1");
        RepeatingTasks.Add("Задача-пример 2");
    }

    [RelayCommand] private void GenerateInstruction() { /* … */ }
    [RelayCommand] private void GenerateReport() { /* … */ }
}

/// <summary>POCO для DataGrid</summary>
public class TaskClassification
{
    public string Task { get; set; } = "";
    public string Type { get; set; } = "";
    public double Hours { get; set; }
}
