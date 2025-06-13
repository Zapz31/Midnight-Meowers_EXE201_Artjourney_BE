using BusinessObjects.Models;
using Helpers.DTOs.Module;
using Helpers.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IModuleService
    {
        public Task<ApiResponse<Module>> CreateModuleAsync(CreateModuleRequestDTO requestDTO);
        public Task<ApiResponse<List<BasicModuleGetResponseDTO>>> GetModulesByCourseIdCompleteAsync(long courseId);
    }
}
