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

        public async Task<IEnumerable<Module>> GetModulesByCourseId(long courseId)
        {
            var moduleQuery = new QueryBuilder<Module>()
                .WithPredicate(m => m.CourseId == courseId && m.DeletedAt == null)
                .WithOrderBy(query => query.OrderBy(m => m.ModuleId))
                .Build();

            var modules = await _unitOfWork.GetRepo<Module>().GetAllAsync(moduleQuery);
            return modules;
        }
        
    }
}
