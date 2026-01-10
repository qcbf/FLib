// =================================================={By Qcbf|qcbf@qq.com|2024-1-8}==================================================

// #undef DEBUG

using System;
using System.Diagnostics;

namespace FLib
{
    public struct DebugOnlyString
    {
#if DEBUG || TRACE
        public string Text;
#endif

        public readonly bool IsEmpty =>
#if DEBUG || TRACE
            string.IsNullOrEmpty(Text)
#else
            true
#endif
        ;

        public readonly override string ToString()
        {
            return this;
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void Append(object str)
        {
#if DEBUG || TRACE
            if (Log.Info != null)
                Text += Json5.SerializeToLog(str);
#endif
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void Append(char str)
        {
#if DEBUG || TRACE
            if (Log.Info != null)
                Text += str;
#endif
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void Append(DebugOnlyString str)
        {
#if DEBUG || TRACE
            if (Log.Info != null)
                Text += str;
#endif
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void AppendLine(DebugOnlyString str)
        {
#if DEBUG || TRACE
            if (Log.Info != null)
                Text += str + Environment.NewLine;
#endif
        }

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void Append(ref DebugOnlyString str, string v) => str.Append(v);

        [Conditional("DEBUG"), Conditional("TRACE")]
        public static void AppendLine(ref DebugOnlyString str, string v) => str.AppendLine(v);

        [Conditional("DEBUG"), Conditional("TRACE")]
        public void Set(object obj)
        {
#if DEBUG || TRACE
            if (Log.Info != null)
                Text = obj.ToString();
#endif
        }


        public static implicit operator DebugOnlyString(string v) => new()
        {
#if DEBUG || TRACE
            Text = v
#endif
        };

        public static implicit operator string(DebugOnlyString v) =>
#if DEBUG || TRACE
            v.Text;
#else
            string.Empty;
#endif
    }
}
