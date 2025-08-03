using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Challenge;
using Microsoft.AspNetCore.Http.Extensions;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class ArtworkRepository : IArtworkRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _dbContext;
        public ArtworkRepository(IUnitOfWork unitOfWork, ApplicationDbContext dbContext)
        {
            this._unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task<List<ArtworkViewBasicResponseDTO>> GetAllArtworksByChallengeIdAsync(long challengeId)
        {
            var queryOption = new QueryBuilder<Artwork>()
                .WithTracking(false)
                .WithPredicate(a => a.ChallengeId == challengeId)
                .Build();

            var data = await _unitOfWork.GetRepo<Artwork>().GetAllAsync(queryOption);

            var result = data.Select(a => new ArtworkViewBasicResponseDTO
            {
                Id = a.Id,
                Image = a.Image,
                Title = a.Title
            }).ToList();
            return result;
        }

        public async Task CreateArtworks(List<Artwork> artworks)
        {
            await _unitOfWork.GetRepo<Artwork>().CreateAllAsync(artworks);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Artwork> CreateSingleArtwork(Artwork artwork)
        {
            var createdData = await _unitOfWork.GetRepo<Artwork>().CreateAsync(artwork);
            await _unitOfWork.SaveChangesAsync();
            return createdData;
        }
    }
}
