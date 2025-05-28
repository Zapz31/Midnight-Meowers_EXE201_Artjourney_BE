using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.FileHandler
{
    public class SingleUploadResult
    {
        public bool Success { get; set; }
        public FileUploadInfo? FileInfo { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
