using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IModuleRepository
    {
        public Task<Module> CreateModuleAsync(Module module);
        public Task<IEnumerable<Module>> GetModulesByCourseId(long courseId);
    }
}
