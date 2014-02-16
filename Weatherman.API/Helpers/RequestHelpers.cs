using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weatherman.API
{
    public static class RequestHelpers
    {
        public static string FormatExcludeString(Exclude[] input)
        {
            return string.Join(",", input.Select(i => Enum.GetName(typeof(Exclude), i)));
        }

        public static string FormatExtendString(Extend[] input)
        {
            return string.Join(",", input.Select(i => Enum.GetName(typeof(Extend), i)));
        }
    }
}
