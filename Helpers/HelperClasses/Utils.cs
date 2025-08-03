using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.HelperClasses
{
    public class Utils
    {
        private static readonly string ImageBaseUrl = "https://zapzminio.phrimp.io.vn/";
        public static bool IsValidEnumValue<T>(string value) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Enum.TryParse<T>(value.Trim(), ignoreCase: true, out _);
        }

        public static string GetDeployMinioURL(string presignedUrl)
        {
            Uri uri = new Uri(presignedUrl);
            string pathAndFileName = uri.PathAndQuery.TrimStart('/');
            var result = ImageBaseUrl + pathAndFileName;
            return result;
        }
    }
}
