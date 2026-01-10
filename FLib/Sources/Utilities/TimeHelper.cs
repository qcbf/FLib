// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;

namespace FLib
{
    public readonly struct TimeHelper
    {
        public static TimeHelper Default = new(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        public static uint Timestamp => Default.GetTimestamp();
        public static long TimestampMs => Default.GetTimestampMs();


        public readonly DateTime BaseDate;

        public TimeHelper(DateTime baseDate)
        {
            BaseDate = baseDate;
        }

        public uint GetTimestamp() => DateToTimestamp(DateTime.UtcNow);
        public long GetTimestampMs() => DateToTimestampMs(DateTime.UtcNow);

        /// <summary>
        /// 时间戳转换c#时间
        /// </summary>
        public DateTime TimestampToDate(long timestamp) => BaseDate.AddSeconds(timestamp).ToLocalTime();

        /// <summary>
        /// 时间戳转换c#时间
        /// </summary>
        public DateTime TimestampMSToDate(long timestamp) => BaseDate.AddMilliseconds(timestamp).ToLocalTime();

        /// <summary>
        /// c#时间转换为时间戳
        /// </summary>
        public uint DateToTimestamp(DateTime date)
        {
            if (date.Kind != DateTimeKind.Utc)
                date = date.ToUniversalTime();
            return (uint)((date.Ticks - Default.BaseDate.Ticks) / TimeSpan.TicksPerMinute);
        }

        /// <summary>
        /// c#时间转换为时间戳(毫秒)
        /// </summary>
        public long DateToTimestampMs(DateTime date)
        {
            if (date.Kind != DateTimeKind.Utc)
                date = date.ToUniversalTime();
            return (date.Ticks - Default.BaseDate.Ticks) / TimeSpan.TicksPerMillisecond;
        }
    }
}
