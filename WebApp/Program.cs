using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using App.Domain.Identity;
using App.Domain.Enum;
using App.EF;
using App.Repository.DalUow;
using App.Repository.Impl.ResxImport;
using App.Service.BllUow;
using App.Service.Impl.Assemblies.Importer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApp.Extensions.Builder;
using WebApp.Extensions.Services;
using WebApp.HealthChecks;
using WebApp.Helpers;
using WebApp.Helpers.Translations.Imp;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Redis.Client;
using WebApp.Redis.Client.Impl;


var builder = WebApplication.CreateBuilder(args);

// PostgreSQL Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing DB connection string. Set it via:\n" +
        "- dotnet user-secrets: ConnectionStrings:DefaultConnection (local)\n" +
        "- env var: ConnectionStrings__DefaultConnection (docker)"
    );
}

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

builder.Services
    .AddOptions<RedisOptions>()
    .Bind(builder.Configuration.GetSection("Redis"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
        "Redis:ConnectionString must be set")
    .ValidateOnStart();

builder.Services.AddSingleton<IRedisClient, RedisClient>();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<ITranslationSource, TranslationSource>();
builder.Services.AddSingleton<ITranslationDistributedCache, TranslationDistributedCache>();
builder.Services.AddSingleton<ITranslationCache, TranslationCache>();

// REDIS- https://redis.io/docs/latest/develop/clients/dotnet/connect/
builder.Services.AddScoped<IUITranslationsProvider>(sp =>
{
    var redis = sp.GetRequiredService<ITranslationCache>();
    var langTag = CultureInfo.CurrentUICulture.Name;
    return new UITranslationsProvider(redis, langTag);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

// IDENTITY
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //.AddEntityFrameworkStores<AppDbContext>();
//builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetRateLimitPartitionKey(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("WritePolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetRateLimitPartitionKey(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("AdminPublishPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetRateLimitPartitionKey(context),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."), tags: new[] { "live" })
    .AddCheck<DatabaseReadinessHealthCheck>("postgres", tags: new[] { "ready" })
    .AddCheck<RedisReadinessHealthCheck>(
        "redis",
        tags: new[] { "ready" });
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie handling
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
    


// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/make-content-localizable?view=aspnetcore-9.0
// LOCALIZATION CONFIG
builder.Services.AddAppLocalization(builder.Configuration);

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

// WARM UP REDIS CACHE
await app.WarmupTranslationsCacheAsync();

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
app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready")
});

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

    var roles = new[] { nameof(RoleType.Admin), nameof(RoleType.Reviewer), nameof(RoleType.Translator) };
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

    await EnsureUserAsync("admin@mail.com", nameof(RoleType.Admin));
    await EnsureUserAsync("reviewer@mail.com", nameof(RoleType.Reviewer));
    await EnsureUserAsync("translator@mail.com", nameof(RoleType.Translator));
}

app.Run();

static string GetRateLimitPartitionKey(HttpContext context)
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!string.IsNullOrWhiteSpace(userId))
    {
        return $"user:{userId}";
    }

    var ip = context.Connection.RemoteIpAddress?.ToString();
    if (!string.IsNullOrWhiteSpace(ip))
    {
        return $"ip:{ip}";
    }

    return "anonymous";
}
