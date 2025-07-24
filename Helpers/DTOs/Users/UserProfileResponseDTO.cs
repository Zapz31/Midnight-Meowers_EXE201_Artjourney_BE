using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Users
{
    public class UserProfileResponseDTO
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public AccountStatus Status { get; set; }
        public AccountRole Role { get; set; }
        public long LoginCount { get; set; }
        public string PremiumStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
