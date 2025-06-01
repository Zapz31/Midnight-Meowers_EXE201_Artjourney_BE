using BusinessObjects.Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class RegionRepository : IRegionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        public RegionRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Region> CreateUserAsync(Region region)
        {
            var createdRegion = await _unitOfWork.GetRepo<Region>().CreateAsync(region);
            await _unitOfWork.SaveChangesAsync();
            return createdRegion;
        }
    }
}
