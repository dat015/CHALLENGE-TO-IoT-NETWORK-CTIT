using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Web3IoT.Data;
using Web3IoT.Models;
using Web3IoT.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpClient();

// Add MqttService as Singleton
builder.Services.AddSingleton<MqttService>();

// Add SensorDataService as HostedService
builder.Services.AddHostedService<SensorDataService>();

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireFarmerRole", policy => policy.RequireRole("Farmer"));
});

var app = builder.Build();

// Initialize MqttService and subscribe to all sensors
try
{
    using var scope = app.Services.CreateScope();
    var mqttService = scope.ServiceProvider.GetRequiredService<MqttService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Khởi tạo MQTT service
    await mqttService.InitializeAsync();

    // Subscribe vào tất cả các sensor
    var sensors = await dbContext.Sensors.ToListAsync();
    foreach (var sensor in sensors)
    {
        await mqttService.SubscribeToSensorAsync(sensor.Name);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Lỗi khởi tạo MQTT Service");
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Thêm Farms nếu chưa có
    if (!dbContext.Farms.Any())
    {
        dbContext.Farms.AddRange(
            new Farm
            {

                Name = "Nông trại A",
                Address = "123 Đường Nông Nghiệp, Hà Nội",
                Area = 10.5m,
                Description = "Nông trại trồng lúa",
                CreatedAt = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                UserId = 1
            },
            new Farm
            {

                Name = "Nông trại B",
                Address = "456 Đường Cây Trồng, TP.HCM",
                Area = 15.0m,
                Description = "Nông trại rau sạch",
                CreatedAt = DateTime.UtcNow,
                LastUpdate = DateTime.UtcNow,
                UserId = 1
            }
        );
        dbContext.SaveChanges();
    }

    
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
