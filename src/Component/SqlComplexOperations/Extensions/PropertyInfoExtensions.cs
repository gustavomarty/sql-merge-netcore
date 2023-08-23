using SqlComplexOperations.Attributes;
using System.Reflection;

namespace SqlComplexOperations.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static string GetPropName(this PropertyInfo propertyInfo, bool useAttr)
        {
            if(!useAttr)
            {
                return propertyInfo.Name;
            }

            var attribute = propertyInfo.GetCustomAttribute<PropertyNameAttribute>(true);

            if(attribute == null)
            {
                return string.Empty;
            }

            var value = attribute.PropertyName;
            return value;
        }
    }
}
