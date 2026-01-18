using Endpoint.Engineering.ALaCarte.Core;
using Endpoint.Engineering.ALaCarte.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure database path in user's home directory to persist across CLI installations
var userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var appDataDirectory = Path.Combine(userHomeDirectory, ".endpoint-alacarte");
Directory.CreateDirectory(appDataDirectory);
var databasePath = Path.Combine(appDataDirectory, "ALaCarte.db");

// Add services to the container.
builder.Services.AddDbContext<ALaCarteContext>(options =>
    options.UseSqlite($"Data Source={databasePath}",
        b => b.MigrationsAssembly("Endpoint.Engineering.ALaCarte.Infrastructure")));

builder.Services.AddScoped<IALaCarteContext>(provider => provider.GetRequiredService<ALaCarteContext>());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply pending migrations automatically to ensure database schema is up-to-date
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ALaCarteContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
