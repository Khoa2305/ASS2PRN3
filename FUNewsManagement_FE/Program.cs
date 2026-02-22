using Polly;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// Offline Mode & Background Worker
builder.Services.AddSingleton<FUNewsManagement_FE.Services.IOfflineStateService, FUNewsManagement_FE.Services.OfflineStateService>();
builder.Services.AddHostedService<FUNewsManagement_FE.Workers.DashboardSyncWorker>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Services
builder.Services.AddTransient<FUNewsManagement_FE.Services.ITokenService, FUNewsManagement_FE.Services.TokenService>();

// Register DelegatingHandlers
builder.Services.AddTransient<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>();
builder.Services.AddTransient<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>();

// Polly Retry Policy: Wait and Retry (3 times: 1s, 2s, 4s)
var retryPolicy = Polly.Extensions.Http.HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

// Register Named Clients
builder.Services.AddHttpClient("Assignment1", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:Assignment1"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("AnalyticsAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:AnalyticsAPI"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient("AIAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:AIAPI"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

// Register Typed Clients (keeping them to not break existing controllers)
builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.ICoreApiClient, FUNewsManagement_FE.Clients.CoreApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:Assignment1"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.IAnalyticsApiClient, FUNewsManagement_FE.Clients.AnalyticsApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:AnalyticsAPI"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.IAIApiClient, FUNewsManagement_FE.Clients.AIApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:AIAPI"]!);
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
