using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("user_module_infos")]
    public class UserModuleInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("info_id")]
        public long InfoId { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_in")]
        public TimeSpan? CompletedIn { get; set; }

        // Navigation properties
        [Column("user_id")]
        [ForeignKey("ModuleUser")]
        public long UserId { get; set; }
        public virtual User ModuleUser { get; set; } = null!;

        [Column("module_id")]
        [ForeignKey("Module")]
        public long ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

    }
}
/*
 table user_module_infos{
  info_id long [primary key, increment] ==
  is_completed bool ==
  completed_at datetime ==
  completed_in datetime ==
  user_id long [ref: > users.id] ==
  module_id long [ref: > modules.module_id]
}
 */
