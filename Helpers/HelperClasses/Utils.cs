using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.HelperClasses
{
    public class Utils
    {
        public static bool IsValidEnumValue<T>(string value) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Enum.TryParse<T>(value.Trim(), ignoreCase: true, out _);
        }

    }
}
