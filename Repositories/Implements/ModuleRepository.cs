using BusinessObjects.Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implements
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IUnitOfWork _unitOfWork;   
        public ModuleRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Module> CreateModuleAsync(Module module)
        {
            var createdModule = await _unitOfWork.GetRepo<Module>().CreateAsync(module);
            await _unitOfWork.SaveChangesAsync();
            return createdModule;
        }
    }
}
