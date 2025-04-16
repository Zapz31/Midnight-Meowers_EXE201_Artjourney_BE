using BusinessObjects.Models;
using Helpers.DTOs.Authentication;

namespace Services.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User account);
        string CreateVerifyToken(User user);
        //string CreateResetToken(ResetPasswordDTO resetPasswordDto);
        RegisterDTO ParseToken(string verifyGmailToken);
    }
}
