using BusinessObjects.Enums;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Helpers.DTOs.Users
{
    public class NewUpdateUserDTO
    {
        public long? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public Gender? Gender {  get; set; }
        public string? PhoneNumber { get; set; }
        [JsonIgnore]
        public string? Password { get; set; }
        public DateTime? Birthday { get; set; }
        public AccountStatus? Status { get; set; }
        public string? AvatarUrl { get; set; }

        public NewUpdateUserDTO(User user) 
        {
            UserId = user.UserId;
            Email = user.Email;
            FullName = user.Fullname;
            Gender = user.Gender;
            PhoneNumber = user.PhoneNumber;
            Birthday = user.Birthday;
            Status = user.Status;
            AvatarUrl = user.AvatarUrl;
        }

        public NewUpdateUserDTO() { }

    }
}
