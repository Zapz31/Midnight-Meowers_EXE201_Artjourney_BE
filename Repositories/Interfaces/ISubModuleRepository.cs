using BusinessObjects.Models;
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
    }
}
