using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.UserLearningProgress
{
    public class CreateULPRequestDTO
    {
        public UserLearningProgressStatus? Status { get; set; }
        public decimal? Score { get; set; }
        public long LearningContentId { get; set; }
    }
}
