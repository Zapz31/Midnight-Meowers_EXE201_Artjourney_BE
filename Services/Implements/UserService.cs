using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.HelperClasses;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(
            IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public async Task<User> CreateAccount(RegisterDTO registerDTO)
        {
            var user = new User() 
            { 
                Email = registerDTO.Email,
                Password = PasswordHasher.HashPassword(registerDTO.Password),
                Role = registerDTO.Role,
            };
            var createdUser = await _userRepository.CreateUserAsync(user);
            return createdUser;
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            var foundUser = _userRepository.GetUserByEmailAsync(email);
            return foundUser;
        }
    }
}
