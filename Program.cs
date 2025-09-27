using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using PortalInmobiliario;
using StackExchange.Redis; 
using PortalInmobiliario.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Conexión a SQLite (para tu App principal)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 🔹 Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()   // ✅ Importante: habilitar roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<InmuebleCacheService>();

// 🔹 Configuración de Redis
var redisConnection = builder.Configuration["Redis:ConnectionString"] ??
                      Environment.GetEnvironmentVariable("Redis__ConnectionString");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "PortalInmobiliario:";
});

// Configurar sesión (usando Redis ahora)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Inyectar ConnectionMultiplexer para funcionalidades avanzadas
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

var app = builder.Build();

// 🔹 Migraciones + creación de roles y usuario demo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Crear rol Broker si no existe
    if (!await roleManager.RoleExistsAsync("Broker"))
    {
        await roleManager.CreateAsync(new IdentityRole("Broker"));
    }

    // Crear usuario demo si no existe
    var demoEmail = "broker@demo.com";
    var demoUser = await userManager.FindByEmailAsync(demoEmail);
    if (demoUser == null)
    {
        demoUser = new IdentityUser { UserName = demoEmail, Email = demoEmail, EmailConfirmed = true };
        await userManager.CreateAsync(demoUser, "Passw0rd!");
        await userManager.AddToRoleAsync(demoUser, "Broker");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
