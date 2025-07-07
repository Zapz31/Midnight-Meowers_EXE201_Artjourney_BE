using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ChallengeViewAResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long DurationSeconds { get; set; }
    }
}
