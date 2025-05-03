using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthenService
    {
        Task<AuthenticationResponse?> Register(RegisterDTO registerDto);
        Task<ApiResponse<AuthenticationResponse>> Login(LoginDTO loginDto);
        Task<ApiResponse<User>> CreateOrUpdateUserByEmailAsync(string email, string? name, string? avatar);
    }
}
