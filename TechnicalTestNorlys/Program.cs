using TechnicalTestNorlys;

var builder = WebApplication.CreateBuilder(args);

// Add database configuration
builder.Services.AddDbContext<AppDbContext>();

// Add controller services
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add handler and command services
builder.Services.AddScoped<Handler>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UsePathBase(new PathString("/api"));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Route Service API v1");
    options.RoutePrefix = string.Empty;
});

// Map controllers
app.MapControllers();

app.Run();