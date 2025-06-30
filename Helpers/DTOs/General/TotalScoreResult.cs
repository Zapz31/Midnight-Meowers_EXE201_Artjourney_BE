using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.General
{
    [Keyless]
    public class TotalScoreResult
    {
        [Column("total_score")]
        public decimal TotalScore { get; set; }
    }
}
