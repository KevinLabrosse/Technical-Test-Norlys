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
    public DbSet<Person> Persons { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured) return;

        // Load configuration from appsettings.json if not provided
        var config = _configuration ?? new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        string user = config["UserId"] ?? "guest";
        string password = config["Password"] ?? string.Empty;

        var connectionString = $"Server=localhost,1433;Database=TestDatabase;User Id={user};Password={password};TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Office entity
        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Location).HasMaxLength(100).IsRequired();
            entity.HasIndex(o => o.Location).IsUnique();
        });
    
        // Configure Person entity with Foreign Key to Office
        modelBuilder.Entity<Person>()
            .HasOne(p => p.Office)
            .WithMany()
            .HasForeignKey(p => p.OfficeId);
    
        // Seed initial office data
        modelBuilder.Entity<Office>().HasData(
            new Office { Id = 1, Location = "Silkeborg", Capacity = 5 },
            new Office { Id = 2, Location = "Esbjerg", Capacity = 10 },
            new Office { Id = 3, Location = "Aalborg", Capacity = 50 }
        );
    }
}