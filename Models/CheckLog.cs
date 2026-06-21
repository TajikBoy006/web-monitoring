namespace WebSentinel.Models;

public class CheckLog
{
	public int Id { get; set; }
	public int SiteId { get; set; }
	public bool IsUp { get; set; }
	public int? StatusCode { get; set; }
	public long ResponseTimeMs { get; set; }
	public string? ErrorMessage { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}