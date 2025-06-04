using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    [Table("region_historical_period")]
    public class RegionHisoricalPeriod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // auto increment
        [Column("id")]
        public long Id { get; set; }

        [Column("region_id")]
        [ForeignKey("Region")]
        public long RegionId;
        public virtual Region Region { get; set; } = null!;

        [Column("historical_period_id")]
        [ForeignKey("HistoricalPeriod")]
        public long HistoricalPeriodId;
        public virtual HistoricalPeriod HistoricalPeriod { get; set; } = null!;


    }
}
