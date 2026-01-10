//=================================================={By Qcbf|qcbf@qq.com|11/19/2024 2:24:06 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public readonly struct WorldAddComponentEvent
    {
        public readonly WorldEntity Entity;
        public readonly WorldComponentHandleEx CompHandle;
        public WorldBase World => Entity.World;
        public ref readonly T RO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref CompHandle.RO<T>(logLevel);
        public ref T RW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref CompHandle.RW<T>(logLevel);

        public WorldAddComponentEvent(WorldComponentHandleEx compHandle)
        {
            CompHandle = compHandle;
            Entity = compHandle.Entity;
        }
    }

    public readonly struct WorldRemoveComponentEvent
    {
        public readonly WorldEntity Entity;
        public readonly WorldComponentHandleEx CompHandle;
        public WorldBase World => Entity.World;
        public ref readonly T RO<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref CompHandle.RO<T>(logLevel);
        public ref T RW<T>(ELogLevel logLevel = ELogLevel.Fatal) where T : IWorldComponentable, new() => ref CompHandle.RW<T>(logLevel);

        public WorldRemoveComponentEvent(WorldComponentHandleEx compHandle)
        {
            CompHandle = compHandle;
            Entity = compHandle.Entity;
        }
    }

    public readonly struct WorldAddComponentEvent<T> where T : IWorldComponentable, new()
    {
        public readonly WorldEntity Entity;
        public readonly WorldComponentHandleEx<T> CompHandle;
        public WorldBase World => Entity.World;
        public ref readonly T RO => ref CompHandle.RO();
        public ref T RW => ref CompHandle.RW();

        public WorldAddComponentEvent(WorldComponentHandleEx<T> compHandle)
        {
            CompHandle = compHandle;
            Entity = compHandle.Entity;
        }
    }

    public readonly struct WorldRemoveComponentEvent<T> where T : IWorldComponentable, new()
    {
        public readonly WorldEntity Entity;
        public readonly WorldComponentHandleEx<T> CompHandle;
        public WorldBase World => Entity.World;
        public ref readonly T RO => ref CompHandle.RO();
        public ref T RW => ref CompHandle.RW();

        public WorldRemoveComponentEvent(WorldComponentHandleEx<T> compHandle)
        {
            CompHandle = compHandle;
            Entity = compHandle.Entity;
        }
    }
}