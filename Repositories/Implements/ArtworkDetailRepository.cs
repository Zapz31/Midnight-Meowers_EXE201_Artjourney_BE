using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.Challenge;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class ArtworkDetailRepository : IArtworkDetailRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public ArtworkDetailRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<ArtworkDetailViewBasicResponseDTO>> GetArtworkDetailByArtworkIds(List<long> artworkIds)
        {
            var queryOption = new QueryBuilder<ArtworkDetail>()
                .WithTracking(false)
                .WithPredicate(ad => artworkIds.Contains(ad.ArtworkId))
                .Build();
            var data = await _unitOfWork.GetRepo<ArtworkDetail>().GetAllAsync(queryOption);
            var result = data.Select(ad => new ArtworkDetailViewBasicResponseDTO
            {
                Id = ad.Id,
                Artist = ad.Artist,
                Period = ad.Period,
                Year = ad.Year,
                CorrectMatch = ad.ArtworkId
            }).ToList();
            return result;
        }

        public async Task CreateArtworkDetails(List<ArtworkDetail> artworkDetails)
        {
            await _unitOfWork.GetRepo<ArtworkDetail>().CreateAllAsync(artworkDetails);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteArtworkDetailsByArtworkIds(List<long> artworkIds)
        {
            var queryOption = new QueryBuilder<ArtworkDetail>()
                .WithTracking(false)
                .WithPredicate(ad => artworkIds.Contains(ad.ArtworkId))
                .Build();
            var artworkDetails = await _unitOfWork.GetRepo<ArtworkDetail>().GetAllAsync(queryOption);
            await _unitOfWork.GetRepo<ArtworkDetail>().DeleteAllAsync(artworkDetails.ToList());
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
