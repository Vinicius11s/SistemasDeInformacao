using System.Text;
using Agile360.API.Middleware;
using Agile360.API.MultiTenancy;
using Agile360.Application;
using Agile360.Domain.Interfaces;
using Agile360.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", "Agile360.API")
      .WriteTo.Console(
          outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});

// JWT (Story 1.3)
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "authenticated";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtSecret),
            IssuerSigningKey = string.IsNullOrEmpty(jwtSecret) ? null : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// Rate limiting (Story 1.3)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth-login", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 5;
    });
    options.AddFixedWindowLimiter("auth-register", config =>
    {
        config.Window = TimeSpan.FromHours(1);
        config.PermitLimit = 3;
    });
    options.AddFixedWindowLimiter("auth-forgot", config =>
    {
        config.Window = TimeSpan.FromHours(1);
        config.PermitLimit = 3;
    });
});

// CORS
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Agile360 API",
        Version = "v1",
        Description = "Agile360 – CRM Jurídico API"
    });
});

// Webhook auth (Story 1.4)
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));

// Tenant (Story 1.2); JWT via ICurrentUserService (Story 1.3)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Application & Infrastructure (DbContext, MediatR, Auth, etc.)
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Security headers (Story 1.3)
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    if (app.Environment.IsProduction())
        context.Response.Headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains";
    await next();
});

// Global exception handling (must be early in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<WebhookAuthMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

// Correlation ID for logging
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Agile360 API v1"));
}

app.UseCors();
app.MapControllers();

// Health checks endpoint (also available via controller /api/health)
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting Agile360 API");
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Required for integration tests (WebApplicationFactory)
public partial class Program { }
