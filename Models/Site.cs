namespace WebSentinel.Models;

public enum SiteStatus
{
	Unknown,
	Up,
	Down
}

public class Site
{
	public int Id { get; set; }
	public string Url { get; set; } = "";

	public SiteStatus Status { get; set; } = SiteStatus.Unknown;

	public int? LastStatusCode { get; set; }

	public long? LastResponseTimeMs { get; set; }

	public DateTime? LastChecked { get; set; }
}