using BusinessObjects.Models;
using Repositories.Interfaces;
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

    }
}
