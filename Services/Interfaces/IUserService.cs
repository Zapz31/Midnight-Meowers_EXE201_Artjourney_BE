using BusinessObjects.Models;
using Helpers.DTOs.Authentication;
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
    }
}
