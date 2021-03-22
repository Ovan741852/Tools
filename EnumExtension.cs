using System;
using System.Collections.Generic;

namespace UNONE.Foundation.Utilities
{
    public static class EnumExtension
    {
        private static Dictionary<Enum, string> _mapping =
            new Dictionary<Enum, string>();

        public static string GetCachedString(this Enum value)
        {
            string result;
            if(!_mapping.TryGetValue(value, out result))
            {
                result = value.ToString();
                _mapping[value] = result;
            }
            return result;
        }
    }
}