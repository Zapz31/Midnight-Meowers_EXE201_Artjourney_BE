using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.UserPremiumStatus
{
    public class GetPremiumBasicDTO
    {
        public long? Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public DateTime? SubcriptionAt { get; set; }
        public long UserId { get; set; }

    }
}
