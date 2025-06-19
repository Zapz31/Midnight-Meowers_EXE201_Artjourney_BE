using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.General
{
    [Keyless]
    public class ModuleSubModuleCourseIds
    {
        public long? SubModuleId { get; set; }
        public long? ModuleId { get; set;}
        public long? CourseId { get; set; }
    }
}
