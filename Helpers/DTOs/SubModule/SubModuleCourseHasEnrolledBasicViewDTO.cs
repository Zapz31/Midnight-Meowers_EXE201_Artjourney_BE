using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.SubModule
{
    [Keyless]
    public class SubModuleCourseHasEnrolledBasicViewDTO
    {
        public long SubModuleId { get; set; }
        public bool? IsCompleted { get; set; }
        public long ModuleId { get; set; }
    }
}
