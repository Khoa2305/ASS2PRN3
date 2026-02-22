using Polly;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register DelegatingHandlers
builder.Services.AddTransient<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>();
builder.Services.AddTransient<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>();

// Polly Retry Policy: Wait and Retry (3 times: 1s, 2s, 4s)
var retryPolicy = Polly.Extensions.Http.HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

// Register Typed Clients
builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.ICoreApiClient, FUNewsManagement_FE.Clients.CoreApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5039/");
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.IAnalyticsApiClient, FUNewsManagement_FE.Clients.AnalyticsApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7123/"); // Default Analytics API port, update if different
})
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.TokenRefreshDelegatingHandler>()
.AddHttpMessageHandler<FUNewsManagement_FE.HttpHandlers.LoggingDelegatingHandler>()
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<FUNewsManagement_FE.Clients.IAIApiClient, FUNewsManagement_FE.Clients.AIApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7124/"); // Default AI API port, update if different
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
