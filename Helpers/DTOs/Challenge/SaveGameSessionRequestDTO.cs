using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class SaveGameSessionRequestDTO
    {
        public long UserId { get; set; }
        public long ChallengeId { get; set; }
        public int Score { get; set; }
        public long TimeTaken { get; set; }
        public bool IsCompleted { get; set; }
    }
}
