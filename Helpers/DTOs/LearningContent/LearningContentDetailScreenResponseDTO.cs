using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.LearningContent
{
    public class LearningContentDetailScreenResponseDTO
    {
        public long? LearningContentId { get; set; }
        public string? LearningContentTitle {  get; set; }
        public UserLearningProgressStatus? userLearningProgressStatus { get; set; }
    }
}
