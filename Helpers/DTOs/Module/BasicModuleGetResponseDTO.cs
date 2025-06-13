using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.Module
{
    public class BasicModuleGetResponseDTO
    {     
        public long ModuleId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;    
        public string? Description { get; set; }    
        public long? UpdatedBy { get; set; }     
        public long? DeletedBy { get; set; }     
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;       
        public DateTime? UpdatedAt { get; set; }     
        public DateTime? DeletedAt { get; set; }  
        public long CourseId { get; set; }
        public long CreatedBy { get; set; }
    }
}
