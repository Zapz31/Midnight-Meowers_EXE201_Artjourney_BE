using Helpers.DTOs.FileHandler;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IFileHandlerService
    {
        Task<SingleUploadResult> UploadSingleFile(IFormFile file, string bucketName, string courseName, string fileType, int index);
        Task<UploadResult> UploadFiles(List<IFormFile> files, string courseName, string fileType);
    }
}
