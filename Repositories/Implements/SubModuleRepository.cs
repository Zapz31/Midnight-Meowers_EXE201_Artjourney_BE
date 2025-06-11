using BusinessObjects.Models;
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
        public SubModuleRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

    }
}
