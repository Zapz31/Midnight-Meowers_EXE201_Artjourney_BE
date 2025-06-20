using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.General
{
    [Keyless]
    public class QueryResultBFlat
    {
        public long CourseId { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseDescription { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? RegionName { get; set; }
        public string? HistoricalPeriodName { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
