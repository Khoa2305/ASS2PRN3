using Assignment1.Middleware;
using Assignment1.Models;
using Assignment1.Repositories.Imp;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Imp;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddOData(option =>
{
    option.Select().Filter().Count().OrderBy().Expand().AddRouteComponents("odata", ODataEdm.GetEdmModel());
});
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddDbContext<FunewsManagementContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
    );
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", policy =>
    {
        policy
            //.WithOrigins("http://localhost:5173") // domain FE
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        //.AllowCredentials(); // nếu dùng cookie / auth
    });
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();

builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<Assignment1.Services.Interface.IAuthenticationService, Assignment1.Services.Imp.AuthenticationService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("MyCorsPolicy");

app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<Assignment1.Middleware.AuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
public static class ODataEdm
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<NewsArticle>("NewsArticles");
        return builder.GetEdmModel();
    }
}