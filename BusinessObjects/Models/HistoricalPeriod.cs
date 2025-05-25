using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("historical_periods")]
    public class HistoricalPeriod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("historical_period_id")]
        public long HistoricalPeriodId { get; set; }

        [Column("historical_period_name")]
        public string HistoricalPeriodName { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_year")]
        public string? StartYear { get; set; }

        [Column("end_year")] 
        public string? EndYear { get; set;}

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public long? DeletedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt {  get; set; }

        // Navigation properties
        // N - 1
        [Column("created_by")]
        [ForeignKey("CreatedUser")]
        public long CreatedBy { get; set; }

        public virtual User CreatedUser { get; set; } = null!;

        // 1 - N
        [InverseProperty("CourseHistoricalPeriod")]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

/*
 Table historical_periods {
  historical_period_id long [primary key] --
  historical_period_name varchar --
  description varchar --
  start_year varchar --
  end_year varchar --
  created_by long [ref: > users.id] //-- userid cua nguoi tao --
  updated_by long --
  deleted_by long --
  created_at datetime --
  updated_at datetime --
  deleted_at datetime
}
 */
