using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.DTOs.Users;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateAccount(RegisterDTO registerDTO);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateCompleteAccount(User newUser);
        Task<ApiResponse<User>> UpdateUserAsync(NewUpdateUserDTO newUpdateUser);
    }
}
