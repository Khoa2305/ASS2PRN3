using FUNewsManagement_AnalyticsAPI.Client;
using FUNewsManagement_AnalyticsAPI.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── CoreAPI HttpClient ────────────────────────────────────────────────────────
var coreApiBaseUrl = builder.Configuration["CoreApi:BaseUrl"]
    ?? throw new InvalidOperationException("CoreApi:BaseUrl is not configured.");

builder.Services.AddHttpClient<ICoreApiClient, CoreApiClient>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl.TrimEnd('/') + "/");
    client.Timeout     = TimeSpan.FromSeconds(30);
});

// ── Analytics Service ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "FUNewsManagement — AnalyticsAPI",
        Version = "v1",
        Description = "Fetch data from CoreAPI and expose analytics, trending, recommendations, and Excel export."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Paste the CoreAPI access token here (optional — only needed for protected endpoints)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AnalyticsAPI v1"));

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
