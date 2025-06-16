using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.General
{
    public class LearningDataIds
    {
        public List<long> ModuleIds { get; set; } = new List<long>();
        public List<long> SubModuleIds { get; set; } = new List<long>();
        public List<long> LearningContentIds { get; set; } = new List<long>();
    }
}
