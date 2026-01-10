//==================={By Qcbf|qcbf@qq.com|6/6/2021 5:21:00 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public interface IUndoCommandable
    {
        object Owner { get; set; }
        UndoCommander Commander { get; set; }
        void Finish();
        void Initialize();
        void OnBegin();
        void OnEnd();
    }
}
