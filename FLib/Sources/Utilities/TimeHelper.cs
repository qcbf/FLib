// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace FLib
{
    public readonly struct TimeHelper
    {
        private const long UnixEpochTicks = 621355968000000000L; // 1970-01-01 00:00:00 UTC

        public static readonly TimeHelper Default = new(new DateTime(UnixEpochTicks, DateTimeKind.Utc));

        public static uint Timestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (uint)((DateTime.UtcNow.Ticks - UnixEpochTicks) / TimeSpan.TicksPerSecond);
        }

        public static long TimestampMs
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => (DateTime.UtcNow.Ticks - UnixEpochTicks) / TimeSpan.TicksPerMillisecond;
        }

        public readonly long BaseTicks; // 存raw ticks(UTC), 避免DateTime.Ticks的间接访问, 也避开Kind语义歧义

        public TimeHelper(DateTime baseDate)
        {
            BaseTicks = baseDate.Kind == DateTimeKind.Utc ? baseDate.Ticks : baseDate.ToUniversalTime().Ticks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetTimestamp() => (uint)((DateTime.UtcNow.Ticks - BaseTicks) / TimeSpan.TicksPerSecond);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetTimestampMs() => (DateTime.UtcNow.Ticks - BaseTicks) / TimeSpan.TicksPerMillisecond;

        public DateTime TimestampToDateUtc(long timestamp) => new(BaseTicks + timestamp * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        public DateTime TimestampToDate(long timestamp) => TimestampToDateUtc(timestamp).ToLocalTime();

        public DateTime TimestampMsToDateUtc(long timestampMs) => new(BaseTicks + timestampMs * TimeSpan.TicksPerMillisecond, DateTimeKind.Utc);
        public DateTime TimestampMSToDate(long timestamp) => TimestampMsToDateUtc(timestamp).ToLocalTime();

        public uint DateToTimestamp(DateTime date)
        {
            var utcTicks = date.Kind == DateTimeKind.Utc ? date.Ticks : date.ToUniversalTime().Ticks;
            return (uint)((utcTicks - BaseTicks) / TimeSpan.TicksPerSecond);
        }

        public long DateToTimestampMs(DateTime date)
        {
            var utcTicks = date.Kind == DateTimeKind.Utc ? date.Ticks : date.ToUniversalTime().Ticks;
            return (utcTicks - BaseTicks) / TimeSpan.TicksPerMillisecond;
        }

        /// <summary> 格式化时间 </summary>
        public static string FormatSeconds(int seconds, string dayText = "d ", string hourText = "h ", string minuteText = "m ", string secondText = "s")
        {
            return StringFLibUtility.ReleaseStrBufAndResult(FormatSeconds(StringFLibUtility.GetStrBuf(), seconds, dayText, hourText, minuteText, secondText));
        }

        /// <summary> 格式化时间 </summary>
        public static StringBuilder FormatSeconds(StringBuilder strbuf, int seconds, string dayText = "d ", string hourText = "h ", string minuteText = "m ", string secondText = "s")
        {
            if (seconds == 0)
                return strbuf.Append(0).Append(secondText);

            var days = seconds / 86400;
            seconds -= days * 86400;
            var hours = seconds / 3600;
            seconds -= hours * 3600;
            var minutes = seconds / 60;
            seconds -= minutes * 60;

            if (days > 0)
                strbuf.Append(days).Append(dayText);
            if (hours > 0)
                strbuf.Append(hours).Append(hourText);
            if (minutes > 0)
                strbuf.Append(minutes).Append(minuteText);
            if (seconds > 0)
                strbuf.Append(seconds).Append(secondText);
            return strbuf;
        }
    }
}