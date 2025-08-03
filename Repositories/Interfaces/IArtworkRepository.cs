using BusinessObjects.Models;
using Helpers.DTOs.Challenge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IArtworkRepository
    {
        public Task<List<ArtworkViewBasicResponseDTO>> GetAllArtworksByChallengeIdAsync(long challengeId);
        public Task CreateArtworks(List<Artwork> artworks);
        public Task<Artwork> CreateSingleArtwork(Artwork artwork);
    }
}
