using BusinessObjects.Enums;
using Helpers.DTOs.Authentication;
using Helpers.HelperClasses;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        public AuthenticationService(IUserService userService, ITokenService tokenService) 
        {
            _userService = userService;
            _tokenService = tokenService;
        }
        public async Task<AuthenticationResponse?> Register(RegisterDTO registerDto)
        {
            var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return null;
            }
            var createdUser = await _userService.CreateAccount(registerDto);
            return new AuthenticationResponse()
            {
                Token = _tokenService.CreateVerifyToken(createdUser)
            };
        }

        public async Task<AuthenticationResponse?> Login(LoginDTO loginDto)
        {
            var account = await _userService.GetUserByEmailAsync(loginDto.Email);

            if (account == null)
                return null;

            bool isPasswordValid = PasswordHasher.VerifyPassword(
                loginDto.Password,
                account.Password
            );

            if (!isPasswordValid)
                return null;
            if (account.Status == AccountStatus.Banned)
                return null;
            return new AuthenticationResponse { Token = _tokenService.CreateToken(account) };
        }
    }
}
