using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Module
{
    public class CreateModuleRequestDTO
    {
        public string ModuleTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long CourseId { get; set; }
    }
}
