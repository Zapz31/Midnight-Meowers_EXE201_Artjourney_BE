using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("sub_modules")]
    public class SubModule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("sub_module_id")]
        public long SubModuleId { get; set; }

        [Column("sub_module_title")]
        public string? SubModuleTitle { get; set;}

        [Column("video_urls")]
        public string? VideoUrls { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set;}

        [Column("created_by")]
        public long CreatedBy { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set;}

        //Navigation properties
        // N - 1
        [Column("module_id")]
        [ForeignKey("Module")]
        public long ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        // 1 - N 
        [InverseProperty("ContentSubModule")]
        public virtual ICollection<LearningContent> LearningContents { get; set; } = new List<LearningContent>();
    }
}

/*
 table sub_modules {
  sub_module_id long [primary key] ==
  sub_module_title varchar ==
  video_urls varchar // 1 hoặc nhiều video url lưu ở đây ==
  description text ==
  display_order int ==
  is_active bool ==
  created_at datetime ==
  updated_at datetime ==
  created_by long //-- userid cua nguoi tao ==
  updated_by long ==
  module_id long [ref: > modules.module_id]
}
 */