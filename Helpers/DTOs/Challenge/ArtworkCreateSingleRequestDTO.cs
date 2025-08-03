using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ArtworkCreateSingleRequestDTO
    {
        public IFormFile? Image { get; set; }
        public string Title { get; set; } = String.Empty;
        public long ChallengeId { get; set; }
    }
}
