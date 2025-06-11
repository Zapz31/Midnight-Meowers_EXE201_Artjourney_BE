using Helpers.DTOs.LearningContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.SubModule
{
    public class SubModuleCourseDetailScreenResponseDTO
    {
        public string? SubModuleTitle {  get; set; }
        public List<LearningContentDetailScreenResponseDTO> learningContentDetailScreenResponseDTOs { get; set; } = new List<LearningContentDetailScreenResponseDTO>();
    }
}
