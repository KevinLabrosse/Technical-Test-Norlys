using TechnicalTestNorlys;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add database configuration
builder.Services.AddDbContext<AppDbContext>();

// Add controller services
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add other services
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<PersonHandler>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Technical Test Norlys API");
});

// Map controllers
app.MapControllers();

app.Run();