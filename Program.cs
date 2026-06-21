using Microsoft.EntityFrameworkCore;
using WebSentinel.Data;
using WebSentinel.Models;
using WebSentinel.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка подключения к SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("Default")
		?? "Data Source=WebMonitoring.db"));

// Регистрация сервисов приложения
builder.Services.AddHttpClient<SiteChecker>(client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddHttpClient<TelegramNotifier>();
builder.Services.AddHostedService<MonitorWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Автоматическое создание БД при первом запуске
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();

var sitesApi = app.MapGroup("/api/sites");

sitesApi.MapGet("/", (AppDbContext db) =>
	db.Sites.ToListAsync());

sitesApi.MapPost("/", async (AppDbContext db, SiteChecker checker, Site site) =>
{
	if (string.IsNullOrWhiteSpace(site.Url))
		return Results.BadRequest("URL не может быть пустым");

	site.Status = SiteStatus.Unknown;

	db.Sites.Add(site);
	await db.SaveChangesAsync();

	// Сразу проверяем новый сайт, не дожидаясь фонового таймера
	var log = await checker.CheckAsync(site);
	site.Status = log.IsUp ? SiteStatus.Up : SiteStatus.Down;
	site.LastStatusCode = log.StatusCode;
	site.LastResponseTimeMs = log.ResponseTimeMs;
	site.LastChecked = log.Timestamp;
	db.Logs.Add(log);
	await db.SaveChangesAsync();

	return Results.Created($"/api/sites/{site.Id}", site);
});

sitesApi.MapDelete("/{id}", async (AppDbContext db, int id) =>
{
	var site = await db.Sites.FindAsync(id);
	if (site is null)
		return Results.NotFound();

	db.Sites.Remove(site);
	await db.SaveChangesAsync();

	return Results.NoContent();
});

sitesApi.MapGet("/{id}/logs", (AppDbContext db, int id) =>
	db.Logs.Where(l => l.SiteId == id)
		   .OrderByDescending(l => l.Timestamp)
		   .Take(50)
		   .ToListAsync());

app.Run();