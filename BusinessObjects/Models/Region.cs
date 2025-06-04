using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("regions")]
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("region_id")]
        public long RegionId { get; set; }

        [Column("region_name")]
        public string RegionName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public long? DeletedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set;} = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get;set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        // N - 1
        [Column("created_by")]
        [ForeignKey("CreatedBy")]
        public long CreatedBy { get; set; }

        public virtual User CreatedUser { get; set; } = null!;

        // 1 -N 
        [InverseProperty("CourseRegion")]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        [InverseProperty("Region")]
        public virtual ICollection<RegionHisoricalPeriod> RegionHisoricalPeriods { get; set; } = new List<RegionHisoricalPeriod>();
    }
}

/*
 Table regions {
  region_id long [primary key]
  region_name varchar
  description varchar
  created_by long //-- userid cua nguoi tao
  updated_by long --
  deleted_by long --
  created_at datetime --
  updated_at datetime
  deleted_at datetime
}
 */
