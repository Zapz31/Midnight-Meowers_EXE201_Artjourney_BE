using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("modules")]
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("module_id")]
        public long ModuleId { get; set; }

        [Column("module_title")]
        public string ModuleTitle { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public long? DeletedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set ; }

        // Navigation properties
        // N - 1
        [Column("course_id")]
        [ForeignKey("ModuleCourse")]
        public long CourseId { get; set; }
        public virtual Course ModuleCourse { get; set; } = null!;

        [Column("created_by")]
        [ForeignKey("ModuleCreator")]
        public long CreatedBy { get; set; }
        public virtual User ModuleCreator { get; set; } = null!;

        // 1 - N 
        [InverseProperty("Module")]
        public virtual ICollection<UserModuleInfo> UserModuleInfos { get; set; } = new List<UserModuleInfo>();

        [InverseProperty("Module")]
        public virtual ICollection<SubModule> SubModules { get; set; } = new List<SubModule>();
    }
}

/*
 table modules {
  module_id long [primary key] ==
  module_title varchar ==
  description varchar ==
  updated_by long ==
  deleted_by long ==
  created_at datetime ==
  updated_at datetime ==
  deleted_at datetime ==
  //navigation
  course_id long [ref: > courses.course_id] ==
  created_by long //-- userid cua nguoi tao
}

 */
