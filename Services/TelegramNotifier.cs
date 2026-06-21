using WebSentinel.Models;

namespace WebSentinel.Services;

public class TelegramNotifier(HttpClient http, IConfiguration configuration, ILogger<TelegramNotifier> logger)
{
	public async Task NotifyStatusChangedAsync(Site site, SiteStatus oldStatus, SiteStatus newStatus, string? errorMessage = null)
	{
		var botToken = configuration["Telegram:BotToken"];
		var chatId = configuration["Telegram:ChatId"];

		if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(chatId))
		{
			logger.LogWarning("⚠️ Telegram Token или ChatId не настроены в appsettings.json");
			return;
		}

		string message = "";

		if (newStatus == SiteStatus.Down)
		{
			message = $"🚨 <b>Сайт недоступен!</b>\n\n" +
					  $"🔗 {site.Url}\n\n" +
					  $"❌ <b>Ошибка:</b> {errorMessage ?? "Не удалось установить соединение"}";
		}
		else if (newStatus == SiteStatus.Up)
		{
			message = $"✅ <b>Сайт снова в сети!</b>\n\n" +
					  $"🔗 {site.Url}\n" +
					  $"⚡ <b>Время ответа:</b> {site.LastResponseTimeMs} мс\n" +
					  $"🔢 <b>Код:</b> {site.LastStatusCode}";
		}
		else
		{
			message = $"🔄 Статус сайта {site.Url} изменился на {newStatus}";
		}

		string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(message)}&parse_mode=HTML";

		try
		{
			var response = await http.GetAsync(url);
			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				logger.LogError("❌ Telegram API вернул ошибку: {Error}", errorContent);
			}
			else
			{
				logger.LogInformation("🚀 Уведомление успешно отправлено в Telegram для {Url}", site.Url);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Ошибка отправки уведомления в Telegram");
		}
	}
}