using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ArtworkDetailCreateRequestDTO
    {
        public string Artist { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public long ArtworkId { get; set; }
    }
}
