// Services/DelegatorApiClient.cs
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus_WPF.Services;

/// <summary>HTTP-клиент к FastAPI BOT.DELEGATOR</summary>
public class DelegatorApiClient
{
    private const string BaseUrl = "https://190c-84-54-78-108.ngrok-free.app";

    // единый HttpClient
    private static readonly Lazy<HttpClient> Shared = new(() =>
    {
        var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return http;
    });

    private readonly HttpClient _http;
    public DelegatorApiClient(HttpClient? injected = null) =>
        _http = injected ?? Shared.Value;

    // ---------- DTO ----------
    public record AnalyzeRequest(string source, string email, object filters);
    public record ClassificationItem(string task, string type, double hours);
    public record AnalyzeResponse(string[] repeating_tasks,
                                  ClassificationItem[] classifications);
    // --------------------------

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>POST /api/analyze/</summary>
    public async Task<AnalyzeResponse?> AnalyzeAsync(
        AnalyzeRequest body,
        string csrfToken,
        string? cookie = null)
    {
        // заголовки
        _http.DefaultRequestHeaders.Remove("X-CSRFTOKEN");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("X-CSRFTOKEN", csrfToken);

        _http.DefaultRequestHeaders.Remove("Cookie");
        if (!string.IsNullOrWhiteSpace(cookie))
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookie);

        // запрос
        using var resp = await _http.PostAsJsonAsync("/api/analyze/", body);

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"API error {(int)resp.StatusCode}: {err}");
        }

        return await resp.Content.ReadFromJsonAsync<AnalyzeResponse>(_jsonOptions);
    }
}
