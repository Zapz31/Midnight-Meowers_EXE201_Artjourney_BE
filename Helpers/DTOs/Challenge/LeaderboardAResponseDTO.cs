using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class LeaderboardAResponseDTO
    {
        public int Rank { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public long HighestScore { get; set; }
        public long TimeTaken { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
