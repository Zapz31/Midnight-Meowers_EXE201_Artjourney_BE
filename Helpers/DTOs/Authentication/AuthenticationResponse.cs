using Helpers.DTOs.Users;
using System.Text.Json.Serialization;

namespace Helpers.DTOs.Authentication
{
    public class AuthenticationResponse
    {
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
        public NewUpdateUserDTO? UserDTO { get; set; }
    }
}
