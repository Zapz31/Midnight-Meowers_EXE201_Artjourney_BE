using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ChallengeLeaderboardResponseDTO
    {
        public long ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public List<LeaderboardAResponseDTO>? Leaderboard { get; set; }
    }
}
