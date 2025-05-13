using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BusinessObjects.Models
{
    [Table("users")] // <--- Mapping bảng
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("user_id")]
        public long UserId { get; set; }

        [Column("fullname")]
        public string Fullname { get; set; } = string.Empty;

        [Column("role")]
        public AccountRole Role { get; set; } = AccountRole.Learner; // enum → string

        [Column("gender")]
        public Gender Gender { get; set; } = Gender.Other;

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; } = String.Empty;

        [Column("birthday")]
        public DateTime Birthday { get; set; } = DateTime.MinValue;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("banned_at")]
        public DateTime? BannedAt { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; } = "https://www.svgrepo.com/show/452030/avatar-default.svg";

        [Column("status")]
        public AccountStatus Status { get; set; } = AccountStatus.Pending;  // enum → string

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

    }
}
