using DAOs;
using Helpers.BackgroundService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using Net.payOS;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Implements;
using Services.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

namespace Artjouney_BE
{
    public class ConfigService
    {
        private readonly IConfiguration _configuration;

        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add services to the container.
            // Ignore Cycles when converting Json objects which has foreign key
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            });

           //PayOS
           PayOS payOS = new PayOS(_configuration["PayOS:PAYOS_CLIENT_ID"] ?? throw new Exception("Cannot find environment"),
                   _configuration["PayOS:PAYOS_API_KEY"] ?? throw new Exception("Cannot find environment"),
                   _configuration["PayOS:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Cannot find environment"));

            services.AddSingleton(payOS);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Register DbContext use PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpContextAccessor();

            // Add jwt authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = _configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = _configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"] ?? "Error when getting JWT SigningKey")
        ),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["TK"]; // Lấy token từ cookie
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
                // Thêm xử lý cho token hết hạn
                //context.Response.StatusCode = 401; // Unauthorized
                //context.Response.ContentType = "application/json";
                //var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Token đã hết hạn" });
                //context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
})
//.AddCookie("ExternalCookies", options =>
//{
//    options.Cookie.Name = ".AspNetCore.External";
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
//})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    //options.SignInScheme = "ExternalCookies";
    options.ClientId = _configuration["Google:ClientId"];
    options.ClientSecret = _configuration["Google:ClientSecret"];
    options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
    options.Scope.Add("profile");
    options.CallbackPath = "/signin-google";
    options.BackchannelTimeout = TimeSpan.FromSeconds(60);
    //options.CorrelationCookie.Path = "/";
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
            // minio setting
            services.AddSingleton<IMinioClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new MinioClient()
                    .WithEndpoint(config["MinIO:Endpoint"])
                    .WithCredentials(config["MinIO:AccessKey"], config["MinIO:SecretKey"])
                    .WithSSL(false) // Set to true if using HTTPS
                    .Build();
            });

            // hosted service
            services.AddHostedService<MinioBucketInitializer>();

            // Repository
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
            services.AddScoped<IVerificationInfoRepository, VerificationInfoRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IHistoricalPeriodRepository, HistoricalPeriodRepository>();
            services.AddScoped<IRegionRepository, RegionRepository>();
            services.AddScoped<ICourseReviewRepository, CourseReviewRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<ISubModuleRepository, SubModuleRepository>();
            services.AddScoped<ILearningContentRepository, LearningContentRepository>();
            services.AddScoped<IUserLearningProgressRepository, UserLearningProgressRepository>();
            services.AddScoped<IUserCourseInfoRepository, UserCourseInfoRepository>();
            services.AddScoped<IUserSubModuleInfoRepository, UserSubModuleInfoRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IQuestionOptionRepository, QuestionOptionRepository>();
            services.AddScoped<IQuestionService, QuestionService>();

            // Services
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IMailSenderService, MailSenderService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ILoginHistoryService, LoginHistoryService>();
            services.AddScoped<IMinioService, MinioService>();
            services.AddScoped<IFileHandlerService, FileHandlerService>();
            services.AddScoped<IHistoricalPeriodService, HistoricalPeriodService>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICourseReviewService, CourseReviewService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<ISubModuleService, SubModuleService>();
            services.AddScoped<ILearningContentService, LearningContentService>();
            services.AddScoped<IUserCourseInfoService, UserCourseInfoService>();
            services.AddScoped<IPayOSService, PayOSService>();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                          ForwardedHeaders.XForwardedProto |
                                          ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear(); // Tin tưởng tất cả proxy
            });


        }
    }
}
