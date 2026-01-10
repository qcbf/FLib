//==================={By Qcbf|qcbf@qq.com|12/10/2022 4:30:36 PM}===================

using System;
using System.Collections.Generic;
using System.Globalization;
using FLib;

namespace FLib
{
    public class StringFLibFormatter : IFormatProvider, ICustomFormatter
    {
        public static StringFLibFormatter Main = new();

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format?.StartsWith('%') == true) // 百分比, 每多一个% 就会乘以0.1
            {
                var val = 1f;
                for (var i = 1; i < format.Length; i++)
                {
                    if (format[i] == '%') val *= 0.1f;
                    else break;
                }
                return (val * Convert.ToSingle(arg)).ToString("0.##") + "%";
            }
            if (format?.StartsWith('+') == true)
                return (Convert.ToSingle(arg) + format[1..].AsSpan().ToFloat()).ToString("0.##");
            if (format?.StartsWith('-') == true)
                return (Convert.ToSingle(arg) - format[1..].AsSpan().ToFloat()).ToString("0.##");
            if (format?.StartsWith('*') == true)
                return (Convert.ToSingle(arg) * format[1..].AsSpan().ToFloat()).ToString("0.##");
            if (format?.StartsWith('/') == true)
                return (Convert.ToSingle(arg) / format[1..].AsSpan().ToFloat()).ToString("0.##");
            if (format == "abs")
                return Math.Abs(Convert.ToDouble(arg)).ToString(CultureInfo.InvariantCulture);
            return arg.ToString();
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}
