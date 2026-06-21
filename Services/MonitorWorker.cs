using Microsoft.EntityFrameworkCore;
using WebSentinel.Data;
using WebSentinel.Models;

namespace WebSentinel.Services;

public class MonitorWorker : BackgroundService
{
	private readonly IServiceProvider _services;
	private readonly ILogger<MonitorWorker> _logger;
	private readonly int _intervalSeconds;

	public MonitorWorker(IServiceProvider services, ILogger<MonitorWorker> logger, IConfiguration configuration)
	{
		_services = services;
		_logger = logger;
		_intervalSeconds = configuration.GetValue<int>("Monitor:IntervalSeconds", 60);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("⏱️ Монитор запущен. Проверка каждые {Interval} сек.", _intervalSeconds);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _services.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				var checker = scope.ServiceProvider.GetRequiredService<SiteChecker>();
				var notifier = scope.ServiceProvider.GetRequiredService<TelegramNotifier>();

				var sites = await db.Sites.ToListAsync(stoppingToken);

				foreach (var site in sites)
				{
					var oldStatus = site.Status;
					var log = await checker.CheckAsync(site, stoppingToken);

					site.Status = log.IsUp ? SiteStatus.Up : SiteStatus.Down;
					site.LastStatusCode = log.StatusCode;
					site.LastResponseTimeMs = log.ResponseTimeMs;
					site.LastChecked = log.Timestamp;

					db.Logs.Add(log);

					if (oldStatus != site.Status)
					{
						await notifier.NotifyStatusChangedAsync(site, oldStatus, site.Status, log.ErrorMessage);
					}
				}

				await db.SaveChangesAsync(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Ошибка во время выполнения цикла мониторинга.");
			}

			await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
		}
	}
}