using Microsoft.EntityFrameworkCore;
using WebSentinel.Models;

namespace WebSentinel.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Site> Sites => Set<Site>();        // таблица сайтов
	public DbSet<CheckLog> Logs => Set<CheckLog>(); // таблица логов

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<CheckLog>()
			.HasIndex(l => new { l.SiteId, l.Timestamp });

		
	}
}