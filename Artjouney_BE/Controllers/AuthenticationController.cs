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

            var token = await _authenticationService.Login(loginDto);
            if (token == null)
            {
                return Unauthorized("Email, password incorrect or account is banned");
            }
            Response.Cookies.Append("TK", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(3)
            });

            return Ok(new { message = "Login successful" });
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
                return StatusCode(response.Code, response);
            } else
            {
                Response.Cookies.Append("TK", response.Message, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddHours(3)
                });
                response.Message = "2001";
                return StatusCode(response.Code, response);
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
        public IActionResult CheckCookieMidd()
        {
            var cookieEmail = _currentUserService.Email;
            
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
                Email = cookieEmail
            });
        }

    }
}
