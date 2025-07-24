using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Users
{
    public class UpdateUserProfileRequestDTO
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        public Gender? Gender { get; set; }

        [Url(ErrorMessage = "Invalid avatar URL format")]
        [StringLength(500, ErrorMessage = "Avatar URL cannot exceed 500 characters")]
        public string? AvatarUrl { get; set; }

        public DateTime? Birthday { get; set; }
    }
}
