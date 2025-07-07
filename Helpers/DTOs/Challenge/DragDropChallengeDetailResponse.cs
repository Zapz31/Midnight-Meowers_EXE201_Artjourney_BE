using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Challenge
{
    public class DragDropChallengeDetailResponse
    {
        public ChallengeViewAResponseDTO? Challenge { get; set; }
        public List<ArtworkViewBasicResponseDTO>? Artworks { get; set;}
        public List<ArtworkDetailViewBasicResponseDTO>? Details { get; set; }

    }
}
