using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.HistoricalPeriod
{
    public class HistoricalPeriodDTO
    {
        public long? HistoricalPeriodId { get; set; }
        public string HistoricalPeriodName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }
        public User? CreatedBy { get; set; }
        public List<long> CreateRequestRegionIds { get; set; } = new List<long>();
        public List<Region> Regions { get; set; } = new List<Region>();
    }
}
