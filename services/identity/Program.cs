using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");

app.MapGet("/.well-known/openid-configuration", () => Results.Ok(new
{
    issuer = builder.Configuration["Keycloak:Authority"],
    authorization_endpoint = $"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/auth",
    token_endpoint = $"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/token",
    userinfo_endpoint = "/userinfo",
    response_types_supported = new[] { "code" },
    scopes_supported = new[] { "openid", "profile", "email" }
}));

app.MapGet("/userinfo", () => Results.Ok(new
{
    sub = Guid.NewGuid().ToString(),
    email = "test@example.com",
    email_verified = true,
    name = "Test User"
}));

app.MapGet("/users", () => Results.Ok(Array.Empty<object>()));
app.MapGet("/users/{id:guid}", (Guid id) => Results.Ok(new { id }));

app.Run();

// Minimal DbContext for now
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

