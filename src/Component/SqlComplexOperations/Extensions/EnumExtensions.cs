using System.Reflection;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace SqlComplexOperations.Extensions
{
    internal static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<string, string> _displayNameCache = new ConcurrentDictionary<string, string>();

        internal static string DisplayName(this Enum value)
        {
            var key = $"{value.GetType().FullName}.{value}";

            var displayName = _displayNameCache.GetOrAdd(key, x =>
            {
                var name = (DescriptionAttribute[])value.GetType().GetTypeInfo().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                return name.Length > 0 ? name[0].Description : value.ToString();
            });

            return displayName;
        }
    }
}
