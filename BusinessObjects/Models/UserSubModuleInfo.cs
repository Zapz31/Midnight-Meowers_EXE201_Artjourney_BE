using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_sub_module_infos")]
    public class UserSubModuleInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("info_id")]
        public long InfoId { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("completed_in")]
        public TimeSpan? CompletedIn { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        [Column("sub_module_id")]
        [ForeignKey("SubModule")]
        public long SubModuleId { get; set; }
        public SubModule SubModule { get; set; } = null!;
    }
}
