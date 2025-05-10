using BusinessObjects.Enums;
using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;

            _key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["JWT:SigningKey"] ?? throw new InvalidOperationException()
                )
            );
        }

        //public string CreateResetToken(ResetPasswordDTO resetPasswordDto)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Email, resetPasswordDto.Email),
        //    };
        //    var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(claims),
        //        Expires = DateTime.Now.AddMinutes(10),
        //        SigningCredentials = creds,
        //        Issuer = _config["JWT:Issuer"],
        //        Audience = _config["JWT:Audience"]
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("fullname", user.Fullname),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("userId", user.UserId.ToString()),
                new Claim("avatar", user.AvatarUrl),
                new Claim("status", user.Status.ToString()),
            };
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreateVerifyToken(User user)
        {
            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                //new Claim("fullname", registerDto.Ful),
                //new Claim("password", registerDto.Password),
                //new Claim("role", user.Role.ToString()),
                new Claim("userid", user.UserId.ToString())
                
            };
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(30),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RegisterDTO ParseToken(string verifyGmailToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(verifyGmailToken);
            //string name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
            string password = jwt.Claims.FirstOrDefault(c => c.Type == "password")?.Value ?? "";
            string email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "";
            string roleString = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";
          
            //string status = jwt.Claims.FirstOrDefault(c => c.Type == "status")?.Value ?? "";

            AccountRole role = Enum.TryParse(roleString, out AccountRole parsedRole) ? parsedRole : AccountRole.Learner;

            var registerDto = new RegisterDTO()
            {
                Password = password,
                Role =  role,
                Email = email,
            };
            return registerDto;
        }
    }
}
