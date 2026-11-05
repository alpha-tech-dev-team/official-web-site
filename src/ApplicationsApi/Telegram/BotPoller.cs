using ApplicationsApi.Services;

namespace ApplicationsApi.Telegram;

public class BotPoller(
    IHttpClientFactory httpFactory,
    VerificationService verification,
    TelegramOptions options,
    ILogger<BotPoller> log) : BackgroundService
{
    private static readonly object ShareKeyboard = new
    {
        keyboard = new[] { new[] { new { text = "Поделиться номером", request_contact = true } } },
        resize_keyboard = true,
        one_time_keyboard = true
    };

    private static readonly object RemoveKeyboard = new { remove_keyboard = true };

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var api = new TelegramApi(httpFactory.CreateClient(), options.Token);
        long offset = 0;

        log.LogInformation("Бот запущен, жду сообщения");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var updates = await api.GetUpdatesAsync(offset, ct);

                foreach (var update in updates)
                {
                    offset = update.UpdateId + 1;

                    if (update.Message is not null)
                        await HandleMessageAsync(api, update.Message, ct);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                log.LogWarning("Ошибка опроса Telegram: {Message}", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    private async Task HandleMessageAsync(TelegramApi api, TgMessage msg, CancellationToken ct)
    {
        if (msg.Chat.Type != "private" || msg.From is null || msg.From.IsBot)
            return;

        var chatId = msg.Chat.Id;

        if (msg.Contact is not null)
        {
            var outcome = await verification.CompleteWithContactAsync(
                chatId, msg.From.Id, msg.Contact.UserId, msg.Contact.PhoneNumber);

            var (text, markup) = outcome switch
            {
                ContactOutcome.Verified =>
                    ("Номер подтверждён! Можно вернуться на сайт.", RemoveKeyboard),
                ContactOutcome.NotOwnContact =>
                    ("Это не ваш контакт. Нажмите кнопку и отправьте свой номер.", ShareKeyboard),
                ContactOutcome.InvalidPhone =>
                    ("Принимаются только белорусские мобильные номера.", RemoveKeyboard),
                ContactOutcome.PhoneMismatch =>
                    ("Этот номер не совпадает с номером в заявке. Начните заново на сайте.", RemoveKeyboard),
                ContactOutcome.ApplicationChanged =>
                    ("Номер в заявке изменился. Начните заново на сайте.", RemoveKeyboard),
                _ =>
                    ("Верификацию нужно начать заново на сайте.", RemoveKeyboard)
            };

            await SendAsync(api, chatId, text, markup, ct);
            return;
        }

        var message = msg.Text?.Trim() ?? "";
        if (message.StartsWith("/start "))
        {
            var token = message["/start ".Length..].Trim();

            if (await verification.AttachChatAsync(token, chatId))
                await SendAsync(api, chatId,
                    "Нажмите кнопку ниже, чтобы поделиться номером. Мы сверим его с номером в заявке.",
                    ShareKeyboard, ct);
            else
                await SendAsync(api, chatId, "Ссылка устарела. Начните заново на сайте.", null, ct);
            return;
        }

        await SendAsync(api, chatId, "Верификацию нужно начинать на сайте.", null, ct);
    }

    private async Task SendAsync(TelegramApi api, long chatId, string text, object? markup, CancellationToken ct)
    {
        if (!await api.SendMessageAsync(chatId, text, markup, ct))
            log.LogWarning("Не удалось отправить сообщение в Telegram");
    }
}
