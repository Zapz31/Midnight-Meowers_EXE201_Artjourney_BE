using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.User
{
    public class NewUpdateUserDTO
    {
        public long? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public Gender? Gender {  get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public DateTime? Birthday { get; set; }
        public AccountStatus? Status { get; set; }
        public string? AvatarUrl { get; set; }

    }
}
