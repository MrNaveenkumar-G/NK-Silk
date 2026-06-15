using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application;
using NKSilk.Domain.Entities;
using NKSilk.Infrastructure;
using NKSilk.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC + layered services. String-enum JSON (de)serialization for the REST API.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Output caching for cacheable, anonymous API reads (catalogue/offers).
builder.Services.AddOutputCache(o =>
    o.AddPolicy("catalog", b => b.Expire(TimeSpan.FromSeconds(60)).SetVaryByQuery("*")));

// Liveness/readiness probe for container orchestration.
builder.Services.AddHealthChecks();

// OpenAPI / Swagger for the REST API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "NK Silk API", Version = "v1" });
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

// Background worker: periodic low-stock monitoring (notifies admins).
builder.Services.AddHostedService<NKSilk.Web.Infrastructure.LowStockMonitorService>();

// Cookie authentication for the web app + JWT bearer for the REST API (/api/v1).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".NKSilk.Auth";
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer("Bearer", options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });

builder.Services.AddScoped<NKSilk.Web.Infrastructure.JwtTokenService>();

// Session is used to persist the guest cart cookie key.
builder.Services.AddHttpContextAccessor();
// Ambient user for audit-trail attribution.
builder.Services.AddScoped<NKSilk.Application.Common.Interfaces.ICurrentUser, NKSilk.Web.Infrastructure.HttpCurrentUser>();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".NKSilk.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.IdleTimeout = TimeSpan.FromDays(7);
});

var app = builder.Build();

// Apply migrations + seed demo data at startup (dev convenience).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Relational stores migrate; non-relational (in-memory test) stores just ensure-created.
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Customer>>();
    await AdminSeeder.SeedAsync(db, hasher);
    await VendorSeeder.SeedAsync(db, hasher);
    await RoleSeeder.SeedAsync(db);
    await PromoSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NK Silk API v1"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseOutputCache();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
