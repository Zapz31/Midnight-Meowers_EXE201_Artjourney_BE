using Helpers.DTOs.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using Helpers.HelperClasses;
using BusinessObjects.Models;
using BusinessObjects.Enums;
using NuGet.Common;
using PayPalCheckoutSdk.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Helpers.DTOs.Users;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenService _authenticationService;
        private readonly IMailSenderService _mailSender;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public AuthenticationController(
            IAuthenService authenticationService,
            IMailSenderService emailSender,
            ICurrentUserService currentUserService,
            ILogger<AuthenticationController> logger,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment
        )
        {
            _authenticationService = authenticationService;
            _mailSender = emailSender;
            _currentUserService = currentUserService;
            _logger = logger;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var verifyGmailToken = await _authenticationService.Register(registerDto);
            if (verifyGmailToken == null)
            {
                return Unauthorized("Email already be used");
            }     
            return Ok();
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string? userAgent = Request.Headers.UserAgent.ToString();
            ApiResponse<AuthenticationResponse> response = await _authenticationService.Login(loginDto, ipAddress, userAgent);
            if (response.Status.Equals(ResponseStatus.Error))
            {
                return StatusCode(response.Code, response);
            }
            Response.Cookies.Append("TK", response.Data.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            return StatusCode(response.Code, response);
        }

        [HttpGet("google-signin")]
        public IActionResult LoginWithGoogle()
        {
            // B4: Tạo state và RedirectUri
            _logger.LogInformation("Bắt đầu yêu cầu đăng nhập Google OAuth");
            var properties = new AuthenticationProperties
            {
                RedirectUri = _configuration["Google:RedirectURI"],
                IsPersistent = true
            };
            _logger.LogInformation("Chuyển hướng đến Google với RedirectUri: {RedirectUri}", properties.RedirectUri);
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleCallback()
        {
            // B8: Middleware đã xác thực, lấy thông tin người dùng
            _logger.LogInformation("Nhận callback từ Google OAuth");
            var authResult = await HttpContext.AuthenticateAsync();

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                _logger.LogError("Xác thực Google thất bại. Succeeded: {Succeeded}, Principal: {Principal}",
                    authResult.Succeeded, authResult.Principal == null ? "null" : "not null");
                return Unauthorized("Xác thực thất bại.");
            }

            _logger.LogInformation("Xác thực Google thành công, lấy thông tin người dùng");
            var claims = authResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatar = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

            _logger.LogInformation("Thông tin người dùng từ Google: Email={Email}, Name={Name}, GoogleId={GoogleId}, Avatar={Avatar}",
                email, name, googleId, avatar);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                _logger.LogError("Không lấy được thông tin người dùng từ Google. Email: {Email}, GoogleId: {GoogleId}", email, googleId);
                return BadRequest("Không lấy được thông tin người dùng từ Google");
            }


            // B12–B15: Kiểm tra và cập nhật hoặc tạo người dùng trong DB
            _logger.LogInformation("Gọi dịch vụ CreateOrUpdateUserByEmailAsync với Email={Email}", email);
            ApiResponse<User> response = await _authenticationService.CreateOrUpdateUserByEmailAsync(email, name, avatar);
            if (response.Status.Equals(ResponseStatus.Error))
            {
                _logger.LogError("Tạo hoặc cập nhật người dùng thất bại. Status: {Status}, Code: {Code}, Message: {Message}",
                    response.Status, response.Code, response.Message);
                return Redirect("https://tnhaan20.github.io/ArtJourney/signin-google?issignin=false&errormsg=1006");
            } else
            {
                _logger.LogInformation("Tạo hoặc cập nhật người dùng thành công. Lưu token vào cookie");
                Response.Cookies.Append("TK", response.Message, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30)
                });
                response.Message = "2001";
                //return StatusCode(response.Code, response);
                _logger.LogInformation("Chuyển hướng về frontend với issignin=true");
                return Redirect("https://tnhaan20.github.io/ArtJourney/signin-google?issignin=true");
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("TK", new CookieOptions
            {
                HttpOnly = true,
                Secure = _hostEnvironment.IsProduction(),
                SameSite = _hostEnvironment.IsProduction() ? SameSiteMode.None : SameSiteMode.Lax
            });

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("check-cookie")]
        public IActionResult CheckCookie()
        {
            // Kiểm tra xem cookie "TK" có tồn tại không
            if (!Request.Cookies.TryGetValue("TK", out var token) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new
                {
                    Message = "Cookie not found or empty"
                });
            }

            // Nếu có cookie, trả lại giá trị token
            return Ok(new
            {
                Message = "Cookie received",
                Token = token
            });
        }

        [HttpGet("check-cookie-middleware")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CheckCookieMidd()
        {
            var cookieEmail = _currentUserService.Email;
            var cookieId = _currentUserService.AccountId;
            
            if (string.IsNullOrEmpty(cookieEmail))
            {
                return BadRequest(new
                {
                    Message = "Check cookie throw middleware false"
                });
            }

            // Nếu có cookie, trả lại giá trị token
            return Ok(new
            {
                Message = "Check cookie throw middleware success !!!",
                Email = cookieEmail,
                CookieId = cookieId
            });
        }

        [HttpGet("protected")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ProtectedEndpoint()
        {
            // Chỉ người dùng có token hợp lệ mới có thể truy cập endpoint này
            return Ok(new { message = "Xin chào người dùng đã xác thực!" });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<IActionResult> GetUserByIDAuthAsync()
        {
            var userId = _currentUserService.AccountId;

            ApiResponse<NewUpdateUserDTO?> response = await _authenticationService.getUserByIdAuthAsync(userId);
            if (response.Status.Equals(ResponseStatus.Error))
            {
                return StatusCode(response.Code, response);
            }
            return StatusCode(response.Code, response);
        }

        [HttpGet("email-verification")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SendVerificationEmail()
        {
            //ApiResponse<int> response = await _authenticationService.TestDeleteVerificationInfosByEmail(newUpdateUserDTO.Email);
            ApiResponse<string> response = await _authenticationService.SendVerificationEmail();
            return StatusCode(response.Code, response);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string v)
        {
            ApiResponse<string> response = await _authenticationService.VerifyEmail(v);
            return StatusCode(response.Code, response);

        }

    }
}
