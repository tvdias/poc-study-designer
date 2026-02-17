using System;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    public static class EnumHelpers
    {
        public static T? GetEnum<T>(this string value)
            where T : struct, Enum
        {
            if (!Enum.TryParse<T>(value, true, out var enumValue) ||
                !Enum.IsDefined(typeof(T), enumValue))
            {
                return null;
            }

            return enumValue;
        }
    }
}
