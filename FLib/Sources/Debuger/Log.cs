// ==================== qcbf@qq.com | 2025-08-02 ====================

#nullable enable
// ReSharper disable UnassignedReadonlyField

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FLib;

namespace FLib
{
    public enum ELogLevel : sbyte
    {
        None = -1,
        Verbose,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }

    public class Log
    {
        public static event Action<Log, string>? GlobalOutputHandler;

        public static readonly Log?[] GlobalLogs =
        {
#if DEBUG
            null, new(ELogLevel.Debug), new(ELogLevel.Info), new(ELogLevel.Warn),
#else
            null, null, null, null,
#endif
            new(ELogLevel.Error), new FatalLog()
        };

        public static Log? Verbose => GlobalLogs[(int)ELogLevel.Verbose];
        public static Log? Debug => GlobalLogs[(int)ELogLevel.Debug];
        public static Log? Info => GlobalLogs[(int)ELogLevel.Info];
        public static Log? Warn => GlobalLogs[(int)ELogLevel.Warn];
        public static Log? Error => GlobalLogs[(int)ELogLevel.Error];
        public static Log? Fatal => GlobalLogs[(int)ELogLevel.Fatal];
        public static bool IsEnableInfo => Info != null;

        public event Action<string>? OutputHandler;
        public ELogLevel Level;
        public EOption Options = EOption.AppendDate;

        public Log(ELogLevel level) => Level = level;

        [Flags]
        public enum EOption
        {
            None = 0,
            AppendDate = 0x1,
            AppendCallStack = 0x2,
            AppendThreadId = 0x4,
        }

        private class FatalLog : Log
        {
            public FatalLog(ELogLevel logLevel = ELogLevel.Fatal) : base(logLevel)
            {
            }
#if UNITY_PROJ
            [UnityEngine.HideInCallstack]
#endif
            public override void Write(object? content, object? tag1 = null, object? tag2 = null) => throw new Exception(Combine(content, tag1, tag2, Options));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 
        /// </summary>
#if UNITY_PROJ
        [UnityEngine.HideInCallstack]
#endif
        public virtual void Write(object? content, object? tag1 = null, object? tag2 = null)
        {
            var str = Combine(content, tag1, tag2, Options);
            OutputHandler?.Invoke(str);
            GlobalOutputHandler?.Invoke(this, str);
        }

        /// <summary>
        /// 设置日志等级
        /// </summary>
        public static void Set(ELogLevel level)
        {
            for (var i = ELogLevel.Verbose; i <= ELogLevel.Fatal; i++)
                GlobalLogs[(sbyte)i] = i >= level ? GlobalLogs[(sbyte)i] ?? (i == ELogLevel.Fatal ? new FatalLog() : new Log(i)) : null;
            Info?.Write($"set log {level}", nameof(Log));
        }

        /// <summary>
        /// 获取指定等级的日志
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Log? Get(ELogLevel level) => level < 0 ? null : GlobalLogs[(int)level];

        /// <summary>
        /// 获取当前日志等级
        /// </summary>
        public static ELogLevel GetCurrentLevel()
        {
            for (var i = ELogLevel.Verbose; i <= ELogLevel.Fatal; i++)
            {
                if (GlobalLogs[(sbyte)i] != null)
                    return i;
            }

            return ELogLevel.None;
        }

        /// <summary>
        /// 
        /// </summary>
#if UNITY_PROJ
        [UnityEngine.HideInCallstack]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Log? Assert(bool condition, ELogLevel level = ELogLevel.Fatal) => condition ? null : Get(level);

        /// <summary>
        /// 
        /// </summary>
#if UNITY_PROJ
        [UnityEngine.HideInCallstack]
#endif
#pragma warning disable CS8777
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Log? AssertNotNull([NotNull] object? condition, ELogLevel level = ELogLevel.Fatal) => condition == null ? Get(level) : null;

        /// <summary>
        /// 
        /// </summary>
        public static string FormatDatetime(in DateTime dateTime)
        {
#if UNITY_PROJ
            return dateTime.ToString("HH:mm:ss.fff");
#else
            return dateTime.ToString("HH:mm:ss.ff");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Combine(object? content, object? tag1 = null, object? tag2 = null, EOption options = EOption.AppendDate)
        {
            var contentStr = content == null ? string.Empty : Json5.SerializeToLog(content);
            var tag1Str = tag1 == null ? string.Empty : Json5.SerializeToLog(tag1);
            var tag2Str = tag2 == null ? string.Empty : Json5.SerializeToLog(tag2);
            var strbuf = StringFLibUtility.GetStrBuf(tag1Str.Length + contentStr.Length);
            if ((options & EOption.AppendDate) != 0)
                strbuf.Append('[').Append(FormatDatetime(DateTime.Now)).Append(']');
            if ((options & EOption.AppendThreadId) != 0)
                strbuf.Append('[').Append(Environment.CurrentManagedThreadId).Append(']');
            var isHaveTag = false;
            if (tag1Str.Length > 0)
            {
                strbuf.Append('[').Append(tag1Str).Append(']');
                isHaveTag = true;
            }

            if (tag2Str.Length > 0)
            {
                strbuf.Append('[').Append(tag2Str).Append(']');
                isHaveTag = true;
            }

            if (isHaveTag)
                strbuf.Append(' ');
            strbuf.Append(contentStr);
            if ((options & EOption.AppendCallStack) != 0)
                strbuf.AppendLine().AppendLine(GetStackTrace(2, isNeedFileInfo: true));
            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }

        /// <summary>
        /// 
        /// </summary>
        public static string GetStackTrace(int skinFrames = 0, int count = int.MaxValue, string splitChar = "\n", bool isNeedFileInfo = false)
        {
            var frames = new StackTrace(skinFrames + 1, isNeedFileInfo).GetFrames()!;
            count = Math.Min(frames.Length, count);
            var strbuf = StringFLibUtility.GetStrBuf();
            for (var i = 0; i < count; i++)
            {
                var m = frames[i].GetMethod();
                strbuf.Append(m?.DeclaringType).Append('.').Append(m?.Name).Append(':').Append(frames[i].GetFileLineNumber()).Append(splitChar);
            }

            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }

        /// <summary>
        ///
        /// </summary>
        public static void RegisterGlobalUnhandledException(bool isSetObserved = true)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) => Error?.Write(args);
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Error?.Write(args.Exception);
                if (isSetObserved)
                    args.SetObserved();
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ConsoleOutput(Log? log, string text)
        {
            if (log != null)
            {
                if (log.Level == ELogLevel.Warn)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (log.Level >= ELogLevel.Error)
                    Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}