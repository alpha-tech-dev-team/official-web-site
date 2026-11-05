using System.Net.Http.Json;
using System.Text.Json;

namespace ApplicationsApi.Telegram;

public class TelegramApi(HttpClient http, string token)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private string Url(string method) => $"https://api.telegram.org/bot{token}/{method}";

    public async Task<IReadOnlyList<TgUpdate>> GetUpdatesAsync(long offset, CancellationToken ct)
    {
        var response = await http.GetFromJsonAsync<TgResponse<List<TgUpdate>>>(
            Url($"getUpdates?timeout=30&offset={offset}"), Json, ct);

        if (response is null || !response.Ok)
            throw new InvalidOperationException(response?.Description ?? "Пустой ответ Telegram");

        return response.Result ?? new List<TgUpdate>();
    }

    public async Task<bool> SendMessageAsync(long chatId, string text, object? replyMarkup, CancellationToken ct)
    {
        var payload = new Dictionary<string, object?>
        {
            ["chat_id"] = chatId,
            ["text"] = text
        };
        if (replyMarkup is not null)
            payload["reply_markup"] = replyMarkup;

        using var response = await http.PostAsJsonAsync(Url("sendMessage"), payload, ct);
        return response.IsSuccessStatusCode;
    }
}
