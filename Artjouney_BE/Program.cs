using Artjouney_BE;
using Helpers.HelperClasses;
// Artjouney_BE/Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Helpers.HelperClasses; // Your PayOSConfig
using Services.Interfaces;
using Services.Implements;

var builder = WebApplication.CreateBuilder(args);

var configService = new ConfigService(builder.Configuration);
configService.ConfigureServices(builder.Services);

// Configure PayOSConfig
builder.Services.Configure<PayOSConfig>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your custom services
builder.Services.AddScoped<IPayOSService, PayOSService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder =>
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .WithOrigins("http://localhost:5173", "http://localhost:8080"));
}



//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
