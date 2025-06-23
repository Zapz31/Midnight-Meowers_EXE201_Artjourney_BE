using Artjouney_BE;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Information() // Ghi log từ mức Information trở lên
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Giảm log từ Microsoft
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .WriteTo.Console() // Ghi log ra console (hiển thị trong docker logs)
        .WriteTo.File(
            path: "/app/logs/app-.log", // Đường dẫn tuyệt đối trong container| docker: "/app/logs/app-.log" | windows: @"D:\GIAP\logs\artjourney_log\app-.log"
            rollingInterval: RollingInterval.Day, // Tạo tệp log mới mỗi ngày
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );
});

var configService = new ConfigService(builder.Configuration);
configService.ConfigureServices(builder.Services);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://tnhaan20.github.io", 
            "http://localhost:5173", 
            "http://localhost:8080",
            "https://zapz.phrimp.io.vn",
            "http://localhost:19006",
            "http://localhost:8081")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsProduction())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });
}


//Check all cookie was send to backend
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Cookies received: {Cookies}", context.Request.Cookies.Select(c => $"{c.Key}={c.Value}"));

// Ghi log Set-Cookie trong phản hồi
var originalResponse = context.Response.Body;
using var responseBody = new MemoryStream();
context.Response.Body = responseBody;

await next.Invoke();

// Đọc header Set-Cookie
var setCookieHeaders = context.Response.Headers["Set-Cookie"];
if (setCookieHeaders.Any())
{
    logger.LogInformation("Set-Cookie headers in response: {SetCookie}", string.Join("; ", setCookieHeaders));
}

// Copy phản hồi về stream gốc
responseBody.Seek(0, SeekOrigin.Begin);
await responseBody.CopyToAsync(originalResponse);
context.Response.Body = originalResponse;
});

//app.UseHttpsRedirection();

//app.UseCookiePolicy(new CookiePolicyOptions
//{
//    MinimumSameSitePolicy = SameSiteMode.Lax
//});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
