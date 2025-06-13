using BusinessObjects.Models;
using Helpers.DTOs.SubModule;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISubModuleService
    {
        public Task<ApiResponse<SubModule>> CreateSubModuleAsync(CreateSubModuleRequestDTO requestDTO);
        public Task<ApiResponse<List<BasicSubModuleGetResponseDTO>>> GetSubmodulesByModuleId(long moduleId);
    }
}
