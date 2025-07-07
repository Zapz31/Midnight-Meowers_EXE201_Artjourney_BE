using BusinessObjects.Models;
using Helpers.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IArtworkDetailRepository
    {
        public Task<List<ArtworkDetailViewBasicResponseDTO>> GetArtworkDetailByArtworkIds(List<long> artworkIds);
        public Task CreateArtworkDetails(List<ArtworkDetail> artworkDetails);
    }
}
