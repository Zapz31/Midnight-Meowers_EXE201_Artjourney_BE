using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class ArtworkViewBasicResponseDTO
    {
        public long Id { get; set; }
        public string Image { get; set; } = string.Empty;
        public string Title { get; set; } = String.Empty;
    }
}
