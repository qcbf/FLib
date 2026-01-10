//==================={By Qcbf|qcbf@qq.com|8/12/2021 6:31:14 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class BaseUndoCommand<T> : IUndoCommandable
    {
        public T Owner
        {
            get;
            set;
        }

        public UndoCommander Commander
        {
            get;
            set;
        }

        object IUndoCommandable.Owner
        {
            get => Owner;
            set => Owner = (T)value;
        }

        public virtual void Finish()
        {
            OnBegin();
            Commander.OnDoEvent?.Invoke(this);
        }

        public virtual void Initialize() { }
        public abstract void OnBegin();
        public abstract void OnEnd();
    }
}
