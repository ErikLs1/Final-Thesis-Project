using System.Globalization;
using App.Domain.Identity;
using App.EF;
using App.Repository.DalUow;
using App.Service;
using App.Service.BllUow;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IAppUow, AppUow>();
builder.Services.AddScoped<IAppBll, AppBll>();

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
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

var defaultCulture = builder.Configuration["DefaultCulture"] ?? "en"; // default fallback culture if nothing found
var supportedCultureNames = builder.Configuration.GetSection("SupportedCultures").Get<string[]>()!;
var supportedCultures = supportedCultureNames.Select(x => new CultureInfo(x)).ToList(); // culture switching support

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // if nothing is found, use default culture
    options.DefaultRequestCulture = new RequestCulture(defaultCulture);
    // datetime and currency support
    options.SupportedCultures = supportedCultures; // Maybe delete since not relate to ui
    // UI translated strings
    options.SupportedUICultures = supportedCultures;
    // Fallbacks
    options.FallBackToParentCultures = true; // Maybe delete since not related to ui
    options.FallBackToParentUICultures = true;
    

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    { 
        // Order of evaluation
        // add support for ?culture=ru-RU
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});

var app = builder.Build();

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

app.UseRequestLocalization();

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

// TODO: CHNAGE LATER. [TEST IMPL]
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