using Helpers.DTOs.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace Artjouney_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMailSenderService _mailSender;
        private readonly ICurrentUserService _currentUserService;

        public AuthenticationController(
            IAuthenticationService authenticationService,
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
