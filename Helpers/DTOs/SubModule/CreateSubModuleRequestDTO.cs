using BusinessObjects.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.SubModule
{
    public class CreateSubModuleRequestDTO
    {
        public string? SubModuleTitle { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public long ModuleId { get; set; }
    }
}
