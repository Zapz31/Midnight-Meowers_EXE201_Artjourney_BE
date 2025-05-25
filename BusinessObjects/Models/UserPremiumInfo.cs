using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_premium_info")]
    public class UserPremiumInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Column("end_date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Column("status")]
        public UserPremiumStatus Status { get; set; } = UserPremiumStatus.PremiumActive;

        [Column("subcription_at")]
        public DateTime SubcriptionAt { get; set; } = DateTime.UtcNow;

        //Navigate properties
        // N - 1
        [Column("user_id")]
        [ForeignKey("UserPremium")]
        public long UserId { get; set; }
        public virtual User UserPremium { get; set; } = null!;
    }
}

/*
 table user_premium_infos {
  id long [primary key, increment] ==
  user_id long [ref: > users.id] ==
  start_date timestamp ==
  end_date timestamp ==
  status varchar ==
  subcription_at timestamp ==
}
 */
