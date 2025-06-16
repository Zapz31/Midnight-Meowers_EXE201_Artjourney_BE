using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.General
{
    [Keyless]
    public class QueryResultA
    {
        public long module_id { get; set; }
        public long sub_module_id { get; set; }
        public long learning_content_id { get; set; }
    }
}
