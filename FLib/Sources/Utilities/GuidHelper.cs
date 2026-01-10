// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;
using System.Threading;

namespace FLib
{
    public static class GuidHelper
    {
        private static int _step;
        public static readonly TimeHelper BaseDate = new(new DateTime(2025, 8, 1));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxBitWide">整体是多少位 16，32，64</param>
        /// <param name="deviceId">设备id</param>
        /// <param name="deviceBitWide">设备id位宽</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="timestampBitWide">时间戳位宽</param>
        /// <returns></returns>
        public static long Create(int maxBitWide, int deviceId, byte deviceBitWide, long timestamp, int timestampBitWide)
        {
            maxBitWide -= deviceBitWide;
            var device = (ulong)deviceId << maxBitWide;
            maxBitWide -= timestampBitWide;
            var ms = (ulong)timestamp << maxBitWide;
            var step = (uint)(Interlocked.Increment(ref _step) & (1 << maxBitWide) - 1);
            return (long)(device | ms | step);
        }

        public static Guid Create128() => Guid.NewGuid();

        /// <summary>
        /// per milliseconds 32768
        /// year 69
        /// </summary>
        public static long Create64(byte deviceId) => Create(64, deviceId, 8, BaseDate.GetTimestampMs(), 41);

        /// <summary>
        /// per milliseconds 127
        /// year 69
        /// </summary>
        public static long Create64(ushort deviceId) => Create(64, deviceId, 16, BaseDate.GetTimestampMs(), 41);

        /// <summary>
        /// per milliseconds 8388607
        /// year 69
        /// </summary>
        public static long Create64() => Create(64, 0, 0, BaseDate.GetTimestampMs(), 41);

        /// <summary>
        /// per seconds 8
        /// year 17
        /// </summary>
        public static int Create32() => (int)Create(32, 0, 0, BaseDate.GetTimestamp(), 29);

        /// <summary>
        /// per seconds 15
        /// year 8
        /// </summary>
        public static int Create32_15() => (int)Create(32, 0, 0, BaseDate.GetTimestamp(), 28);

        /// <summary>
        /// per seconds 32767
        /// day 1.5
        /// </summary>
        public static int Create32_32767() => (int)Create(32, 0, 0, BaseDate.GetTimestamp(), 17);
    }
}