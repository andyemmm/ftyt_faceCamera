using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ftyt.faceCameraFramework.Enums
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value, bool useChinese)
        {
            if (!useChinese)
                return value.ToString();

            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }
    }
}
