//=================================================={By Qcbf|qcbf@qq.com|11/18/2024 2:39:15 PM}==================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FLib.Worlds;

namespace FLib
{
    public struct PauseCounter
    {
        public byte CurrentPauseCount;
        public readonly bool IsPaused => CurrentPauseCount > 0;
        public readonly override string ToString() => IsPaused ? CurrentPauseCount.ToString() : string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public byte AddPauseCount(int addCount, string name)
        {
            Log.Verbose?.Write($"{name} cur:{CurrentPauseCount} add:{addCount}", nameof(PauseCounter));
            try
            {
                var curCount = (int)CurrentPauseCount;
                curCount += addCount;
                if (curCount is < 0 or > byte.MaxValue) throw new Exception($"{name} {curCount} {addCount} error");
                return CurrentPauseCount = (byte)curCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"pause component[{name}] overflow {CurrentPauseCount} {addCount} {ex}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPause(string name, bool v)
        {
            if (v)
                Pause(name);
            else
                Unpause(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause(string name) => AddPauseCount(1, name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unpause(string name)
        {
            return AddPauseCount(-1, name) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(in PauseCounter v) => v.IsPaused;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte(in PauseCounter v) => v.CurrentPauseCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PauseCounter(in byte v) => new() { CurrentPauseCount = v };
    }
}
