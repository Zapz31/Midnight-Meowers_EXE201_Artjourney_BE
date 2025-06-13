using BusinessObjects.Models;
using DAOs;
using Helpers.DTOs.SubModule;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class SubModuleRepository : ISubModuleRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public SubModuleRepository(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<SubModule> CreateUserAsync(SubModule subModule)
        {
            var createdSubModule = await _unitOfWork.GetRepo<SubModule>().CreateAsync(subModule);
            await _unitOfWork.SaveChangesAsync();
            return createdSubModule;
        }

        public async Task<IEnumerable<SubModule>> GetSubModulesByModuleIds(List<long> moduleIds)
        {
            var subModuleQuery = new QueryBuilder<SubModule>()
                .WithPredicate(sm => moduleIds.Contains(sm.ModuleId) && sm.IsActive)
                .WithOrderBy(query => query.OrderBy(sm => sm.DisplayOrder))
                .Build();

            var subModules = await _unitOfWork.GetRepo<SubModule>().GetAllAsync(subModuleQuery);
            return subModules;
        }

        // Get submodules by module ID
        public async Task<List<BasicSubModuleGetResponseDTO>> GetSubModulesByModuleIdAsync(long moduleId)
        {
            var subModules = await _context.SubModules
                .FromSqlRaw("SELECT * FROM sub_modules sm WHERE sm.module_id = {0} and sm.is_active = true ORDER BY display_order ASC", moduleId)
                .ToListAsync();

            var dtos = subModules.Select(sm => new BasicSubModuleGetResponseDTO
            {
                SubModuleId = sm.SubModuleId,
                SubModuleTitle = sm.SubModuleTitle,
                VideoUrls = sm.VideoUrls,
                Description = sm.Description,
                DisplayOrder = sm.DisplayOrder,
                IsActive = sm.IsActive,
                CreatedAt = sm.CreatedAt,
                UpdatedAt = sm.UpdatedAt,
                CreatedBy = sm.CreatedBy,
                UpdatedBy = sm.UpdatedBy,
                ModuleId = sm.ModuleId
            }).ToList();

            return dtos;
        }

    }
}
