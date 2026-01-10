//==================={By Qcbf|qcbf@qq.com|8/12/2021 6:32:19 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public abstract class BaseValueUndoCommand<T, TValue> : BaseUndoCommand<T>
    {
        public TValue NewValue;
        public TValue OldValue;


        protected abstract TValue Value { get; set; }


        public virtual void Finish(TValue v)
        {
            NewValue = v;
            OldValue = Value;
            base.Finish();
        }

        public override void OnBegin()
        {
            Value = NewValue;
        }

        public override void OnEnd()
        {
            Value = OldValue;
        }
    }
}
