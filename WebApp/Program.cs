using App.Domain.Identity;
using App.EF;
using App.Repository.DalUow;
using App.Repository.Impl.ResxImport;
using App.Service.BllUow;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApp.Extensions.Builder;
using WebApp.Extensions.Configuration;
using WebApp.Extensions.Services;
using WebApp.Helpers;
using WebApp.Redis.Client;
using WebApp.Redis.Client.Impl;
using WebApp.Redis.Services;
using WebApp.Redis.Services.Impl;
using WebApp.Vol2;
using WebApp.Vol2.Importer;
using WebApp.Vol2.Scanner;


var builder = WebApplication.CreateBuilder(args);

// PostgreSQL Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options
            .UseNpgsql(
                connectionString, 
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
    );
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options
            .UseNpgsql(
                connectionString, 
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
    );
}

// UOW, BLL, ETC.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IAppUow, AppUow>();
builder.Services.AddScoped<ResxImportRepository>();
builder.Services.AddScoped<IAppBll, AppBll>();
builder.Services.AddScoped<ResourcesImporter>();

// REDIS- https://redis.io/docs/latest/develop/clients/dotnet/connect/
builder.Services.AddSingleton<IRedisClient, RedisClient>();
builder.Services.AddScoped<IRedisTranslationService, RedisTranslationService>();
builder.Services.AddScoped<IUITranslationsProvider, UITranslationsProvider>();
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddHttpContextAccessor();

// IDENTITY
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //.AddEntityFrameworkStores<AppDbContext>();
//builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/make-content-localizable?view=aspnetcore-9.0
// LOCALIZATION CONFIG
builder.Services.AddAppLocalization(builder.Configuration);

// RESX IMPORT CONFIG
builder.Services.AddScoped<ResxImportRepository>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInit");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    const int maxRetries = 15;
    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Migrate failed (attempt {Attempt}/{Max}). Retrying...", attempt, maxRetries);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}


// RESX IMPORT PIPELINE
await app.RunResxImportAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// LOCALIZATION
app.UseLocalization();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();


// TODO: REFACTOR - HARDCODED USERS
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    var roles = new[] { "Admin", "Reviewer", "Translator" };
    foreach (var r in roles)
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole<Guid>(r));

    async Task EnsureUserAsync(string email, string role)
    {
        var user = await userMgr.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
            await userMgr.CreateAsync(user, "Passw0rd!");
        }

        if (!await userMgr.IsInRoleAsync(user, role))
            await userMgr.AddToRoleAsync(user, role);
    }

    await EnsureUserAsync("admin@mail.com", "Admin");
    await EnsureUserAsync("reviewer@mail.com", "Reviewer");
    await EnsureUserAsync("translator@mail.com", "Translator");
}

app.Run();