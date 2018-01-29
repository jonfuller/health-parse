using System;

namespace HealthParse.Standard.Settings
{
    public static class Coercion
    {
        public static object Coerce(object value, Type targetType)
        {
            if (targetType == typeof(int))
            {
                return CoerceInt32(value);
            }
            if (targetType == typeof(float))
            {
                return CoerceFloat(value);
            }
            if (targetType == typeof(bool))
            {
                return CoerceBool(value);
            }
            return CoerceString(value);
        }

        private static int CoerceInt32(object value)
        {
            if (value is int i) return i;
            if (value is long l) return Convert.ToInt32(l);
            if (value is float f) return Convert.ToInt32(f);
            if (value is double m) return Convert.ToInt32(m);
            if (value is decimal d) return Convert.ToInt32(d);
            if (value is string s && (int.TryParse(s, out var val))) return val;

            return 0;
        }
        private static float CoerceFloat(object value)
        {
            if (value is float f) return f;
            if (value is int i) return Convert.ToSingle(i);
            if (value is long l) return Convert.ToSingle(l);
            if (value is double m) return Convert.ToSingle(m);
            if (value is decimal d) return Convert.ToSingle(d);
            if (value is string s && (float.TryParse(s, out var val))) return val;

            return 0;
        }
        private static bool CoerceBool(object value)
        {
            if (value is bool b) return b;
            if (value is string s) return s.Equals("true", StringComparison.CurrentCultureIgnoreCase);

            return false;
        }
        private static string CoerceString(object value)
        {
            return value.ToString();
        }
    }
}