//==================={By Qcbf|qcbf@qq.com|6/6/2021 5:14:05 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public class UndoCommander
    {
        public Action<IUndoCommandable> OnDoEvent;
        public Action<IUndoCommandable> OnUndoEvent;
        public Action<IUndoCommandable> OnRedoEvent;

        public CircleStack<object> DoCommands;
        public CircleStack<object> UndoCommands;
        public object Owner;



        public bool IsHaveCommand
        {
            get => DoCommands.Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public UndoCommander(object owner, int capacity = 20)
        {
            DoCommands = new CircleStack<object>(capacity);
            UndoCommands = new CircleStack<object>(capacity);
            Owner = owner;
        }

        /// <summary>
        /// 
        /// </summary>
        public UndoCommander SetCapacity(int v)
        {
            DoCommands.SetCapacity(v);
            UndoCommands.SetCapacity(v);
            return this;
        }

        /// <summary>
        /// 推入一条命令,需要在设置值之后调用Finish
        /// </summary>
        public T Do<T>() where T : IUndoCommandable, new()
        {
#pragma warning disable IDE0017 // Simplify object initialization
            var obj = new T();//Pool.Create<T>();
#pragma warning restore IDE0017 // Simplify object initialization
            obj.Owner = Owner;
            obj.Commander = this;
            DoCommands.Push(obj);
            UndoCommands.Clear();
            obj.Initialize();
            return obj;
        }

        /// <summary>
        /// 重新推入最近弹出的一条命令
        /// </summary>
        public void Redo()
        {
            if (UndoCommands.Count > 0)
            {
                var obj = (IUndoCommandable)UndoCommands.Pop();
                obj.OnBegin();
                DoCommands.Push(obj);
                OnRedoEvent?.Invoke(obj);
            }
        }

        /// <summary>
        /// 弹出一条命令
        /// </summary>
        public void Undo()
        {
            if (DoCommands.Count > 0)
            {
                var obj = (IUndoCommandable)DoCommands.Pop();
                obj.OnEnd();
                UndoCommands.Push(obj);
                OnUndoEvent?.Invoke(obj);
            }
        }

        /// <summary>
        /// 移除一条记录的命令
        /// </summary>
        public bool RemoveCommand(IUndoCommandable command)
        {
            for (int i = 0; i < UndoCommands.Count; i++)
            {
                if (UndoCommands[i] == command)
                {
                    UndoCommands.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 清除全部命令
        /// </summary>
        public void Clear()
        {
            UndoCommands.Clear();
            DoCommands.Clear();
        }



    }
}
