using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TechnicalTestNorlys.Models;

namespace TechnicalTestNorlys;

public class AppDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    // Parameterless constructor for EF Core design-time tools
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration? configuration = null)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<Office> Offices { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        var config = _configuration ?? new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        string user = config["UserId"] ?? "guest";
        string password = config["Password"] ?? string.Empty;

        var connectionString = $"Server=localhost,1433;Database=TestDatabase;User Id={user};Password={password};TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);
    }
}