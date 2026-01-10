// =================================================={By Qcbf|qcbf@qq.com|2024-10-22}==================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FLib.Worlds
{
    /// <summary>
    /// 世界
    /// </summary>
    public abstract class WorldBase : WorldEventBase
    {
        public static ValueLinkedList<WorldBase> AllWorlds;
        private static int _worldVersion;
        private static SpinLock _locker;
        
        public WorldComponentManager ComponentMgr;
        public WorldEntityManager EntityMgr;
        public WorldSyncer Syncer;

        public abstract uint Frame { get; protected set; }
        public abstract FNum Time { get; protected set; }
        public virtual WorldHandle Handle { get; private set; }
        public static implicit operator WorldHandle(WorldBase v) => v.Handle;
        public bool IsDisposed => ComponentMgr == null;

        /// <summary>
        /// 
        /// </summary>
        public virtual WorldBase Initialize()
        {
            var isLocking = false;
            _locker.Enter(ref isLocking);
            try
            {
                if (AllWorlds.Count >= ushort.MaxValue) throw new Exception("WorldIndex Overflow");
                while ((ushort)++_worldVersion == 0)
                {
                }
                Handle = new WorldHandle((ushort)AllWorlds.Add(this), (ushort)_worldVersion);
                ComponentMgr = new WorldComponentManager(this);
                EntityMgr = new WorldEntityManager(this);
            }
            finally
            {
                if (isLocking)
                    _locker.Exit(false);
            }
            return this;
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Update()
        {
#if DEBUG
            if (Handle.IsEmpty)
                throw new NullReferenceException();
#endif
            Syncer?.Update();
            ComponentMgr.Update();
        }

        public void WriteToBytes(ref BytesWriter writer)
        {
            BytesPack.Pack(EntityMgr, ref writer);
        }

        public void ReadFromBytes(ref BytesReader reader)
        {
            var temp = EntityMgr;
            BytesPack.Unpack(ref temp, ref reader);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ClearAll()
        {
            EntityMgr.RemoveAllEntity();
            ComponentMgr.ClearAll();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                ClearAll();
            }
            finally
            {
                EntityMgr = null;
                Syncer = null;
                ComponentMgr = null;
                var isLocking = false;
                _locker.Enter(ref isLocking);
                try
                {
                    AllWorlds.RemoveAt(Handle.Index);
                }
                finally
                {
                    if (isLocking)
                        _locker.Exit(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Default : WorldBase
        {
            public FNum DeltaTime = FNum.One / 30;
            public override uint Frame { get; protected set; }
            public override FNum Time { get; protected set; }

            public override void Update()
            {
                ++Frame;
                Time += DeltaTime;
                base.Update();
            }
        }
    }
}
