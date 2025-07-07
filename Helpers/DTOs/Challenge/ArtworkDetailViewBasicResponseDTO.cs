using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ArtworkDetailViewBasicResponseDTO
    {
        public long Id { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public long CorrectMatch { get; set; }
    }
}
