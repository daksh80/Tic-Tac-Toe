var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<backend.Services.GameSessionService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://frontend-zeta-ruby-uipk7d3cap.vercel.app",
                "https://frontend-daksh80s-projects.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();

app.Run();
