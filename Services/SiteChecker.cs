using System.Diagnostics;
using WebSentinel.Models;

namespace WebSentinel.Services;

public class SiteChecker(HttpClient http)
{
	public async Task<CheckLog> CheckAsync(Site site, CancellationToken cancellationToken = default)
	{
		var log = new CheckLog { SiteId = site.Id };
		var startTime = Stopwatch.GetTimestamp();

		try
		{
			using var response = await http.GetAsync(site.Url, cancellationToken);

			log.StatusCode = (int)response.StatusCode;
			log.IsUp = response.IsSuccessStatusCode;

			if (!log.IsUp)
			{
				log.ErrorMessage = $"HTTP {(int)response.StatusCode}";
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			log.IsUp = false;
			log.ErrorMessage = ex.Message;
		}
		finally
		{
			log.ResponseTimeMs = (long)Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;
		}

		return log;
	}
}