using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.ChallengeItem
{
    public class BasicChallengeItemGetResponseDTO
    {
        public long UserId { get; set; }
        public ChallengeItemTypes? ItemTypes { get; set; }
        public string? ItemContent { get; set; }
        public int? ItemOrder { get; set; }
        public string? Hint { get; set; }
        public string? AdditionalData { get; set; }
        public long LearningContentId { get; set; }
    }
}
