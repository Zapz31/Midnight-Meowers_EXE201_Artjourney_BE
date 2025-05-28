using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.DTOs.FileHandler
{
    public class UploadResult
    {
        public List<FileUploadInfo> SuccessfulUploads { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
