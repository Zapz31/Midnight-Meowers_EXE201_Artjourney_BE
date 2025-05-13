using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObjects.Enums;

namespace BusinessObjects.Models
{
    [Table("login_histories")]
    public class LoginHistory
    {
        [Key]
        [Column("login_history_id")]
        public long LoginHistoryId { get; set; }  // INT8 trong PostgreSQL tương ứng với long trong C#

        [Column("user_id")]
        public long? UserId { get; set; }  // Có thể nullable nếu User đã bị xóa

        [Column("login_timestamp")]
        public DateTime LoginTimestamp { get; set; } = DateTime.UtcNow;

        [Column("ip_address")]
        public string? IPAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("login_result")]
        public LoginResult? LoginResult { get; set; }

        [Column("session_id")]
        public string? SessionId { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
