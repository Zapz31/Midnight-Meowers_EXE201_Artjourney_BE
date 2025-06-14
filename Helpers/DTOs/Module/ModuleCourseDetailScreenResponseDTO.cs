using Helpers.DTOs.SubModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Module
{
    public class ModuleCourseDetailScreenResponseDTO
    {
        public long? ModuleId {  get; set; }
        public string? ModuleTitle { get; set; }
        public List<SubModuleCourseDetailScreenResponseDTO> subModuleCourseDetailScreenResponseDTOs { get; set; } = new List<SubModuleCourseDetailScreenResponseDTO>();
    }
}
