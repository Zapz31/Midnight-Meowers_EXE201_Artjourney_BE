using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.SubModule
{
    public class BasicSubModuleGetResponseDTO
    {
        public long SubModuleId { get; set; }
        public string? SubModuleTitle { get; set; }
        public string? VideoUrls { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public long CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long ModuleId { get; set; }
    }
}
