using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.HelperClasses
{
    public class ApiResponse<T>
    {
        public ResponseStatus Status { get; set; } // "success" hoặc "error"
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<ApiError>? Errors { get; set; }
    }

    public class ApiError
    {
        public int Code { get; set; } // Error code
        public string? Field { get; set; }
        public string? Message { get; set; }
    }
}
