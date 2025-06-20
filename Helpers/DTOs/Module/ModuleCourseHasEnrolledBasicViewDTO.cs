using Helpers.DTOs.SubModule;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Module
{
    [Keyless]
    public class ModuleCourseHasEnrolledBasicViewDTO
    {
        public long ModuleId { get; set; }
        public bool IsCompleted { get; set; }
        public long CourseId { get; set; }
        public List<SubModuleCourseHasEnrolledBasicViewDTO> SubModules { get; set; } = new List<SubModuleCourseHasEnrolledBasicViewDTO>();
    }
}
