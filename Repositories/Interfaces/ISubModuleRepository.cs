using BusinessObjects.Models;
using Helpers.DTOs.SubModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISubModuleRepository
    {
        public Task<SubModule> CreateUserAsync(SubModule subModule);
        public Task<IEnumerable<SubModule>> GetSubModulesByModuleIds(List<long> moduleIds);
        public Task<List<BasicSubModuleGetResponseDTO>> GetSubModulesByModuleIdAsync(long moduleId);
        public Task<int> UpdateSubModuleProgress(long userId, long subModuleId, long courseId);
        public Task<int> SoftDeleteSubModuleByIdAsync(long subModuleIdInput);

    }
}
