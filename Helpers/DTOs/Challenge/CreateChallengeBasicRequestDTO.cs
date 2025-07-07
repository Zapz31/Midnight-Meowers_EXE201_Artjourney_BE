using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class CreateChallengeBasicRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ChallengeType { get; set; } = "DragDrop";
        public long DurationSeconds { get; set; }
        public long CourseId { get; set; }

    }
}
