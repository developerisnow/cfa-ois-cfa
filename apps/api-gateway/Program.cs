using Serilog;
using Yarp.ReverseProxy.Configuration;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .ReadFrom.Configuration(ctx.Configuration));

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddCors();

// Rate Limiting (Token Bucket)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = 10,
                AutoReplenishment = true
            }));
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please retry later.", cancellationToken);
    };
});

// Request Size Limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10 MB
    options.ValueLengthLimit = 10485760;
});

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    await next();
});

// CORS
app.UseCors(policy =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
        ?? new[] { "http://localhost:3001", "http://localhost:3002", "http://localhost:3003" };
    
    policy.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

// Request Size Limit
app.Use(async (context, next) =>
{
    context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>()!
        .MaxRequestBodySize = 10485760; // 10 MB
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();

// Rate Limiting
app.UseRateLimiter();

// Health checks
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger"));

// API endpoints (proxied)
app.MapReverseProxy();

app.Run();

