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

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenService _authenticationService;
        private readonly IMailSenderService _mailSender;
        private readonly ICurrentUserService _currentUserService;

        public AuthenticationController(
            IAuthenService authenticationService,
            IMailSenderService emailSender,
            ICurrentUserService currentUserService
        )
        {
            _authenticationService = authenticationService;
            _mailSender = emailSender;
            _currentUserService = currentUserService;
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
            var result = _mailSender.SendVerifyEmail(
                registerDto.Email,
                string.Empty,
                verifyGmailToken.Token,
                "[ArtJourney] – Email verification"
            );

            return Ok(result);
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
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            return StatusCode(response.Code, response);
        }

        [HttpGet("google-signin")]
        public IActionResult LoginWithGoogle()
        {
            // B4: Tạo state và RedirectUri
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback"),
                IsPersistent = true
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            // B8: Middleware đã xác thực, lấy thông tin người dùng
            var authResult = await HttpContext.AuthenticateAsync();

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                return Unauthorized("Xác thực thất bại.");
            }

            var claims = authResult.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatar = claims?.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
                return BadRequest("Không lấy được thông tin người dùng từ Google");

            // B12–B15: Kiểm tra và cập nhật hoặc tạo người dùng trong DB
            ApiResponse<User> response = await _authenticationService.CreateOrUpdateUserByEmailAsync(email, name, avatar);
            if (response.Status.Equals(ResponseStatus.Error))
            {
                return Redirect("http://localhost:5173/signin-google?issignin=false&errormsg=1006");
            } else
            {
                Response.Cookies.Append("TK", response.Message, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(30)
                });
                response.Message = "2001";
                //return StatusCode(response.Code, response);
                return Redirect("http://localhost:5173/signin-google?issignin=true");
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("TK", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
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
