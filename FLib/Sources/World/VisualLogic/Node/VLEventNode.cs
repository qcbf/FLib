//==================={By Qcbf|qcbf@qq.com|5/30/2021 3:24:09 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public class VLEventNode : VLNode
    {
        /// <summary>
        /// 
        /// </summary>
        public VLBaseEventScript Script;

        /// <summary>
        /// 只会触发一次, 走了某个连接线表示成功触发一次
        /// </summary>
        public bool IsTriggerOnce;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActivated;

        public override void Initialize()
        {
            Script.Env = Env;
            Script.Node = this;
            Script.Initialize();
        }

        public override void Handle()
        {
            Activate();
        }

        public void Activate()
        {
            if (IsActivated) return;
            Log.Verbose?.Write($"activate {Uid}|{Env.Uid}", "VLLogic");
            IsActivated = true;
            try
            {
                Script.OnActivate();
            }
            catch (Exception err)
            {
                Log.Error?.Write($"EventNode Activate Exception: {Env.RuntimeData}-> {Env.DebugInfo}-> {Uid}-> {Script?.GetType()}\n{err}");
            }
            VLEnvironment.NodeActionCallback?.Invoke(Env, this, VLEnvironment.ENodeActionType.ActivateEventNode);
        }

        public void Deactivate()
        {
            if (!IsActivated) return;
            Log.Verbose?.Write($"deactivate {Uid}|{Env.Uid}", "VLLogic");
            IsActivated = false;
            try
            {
                Script.OnDeactivate();
            }
            catch (Exception err)
            {
                Log.Error?.Write($"EventNode Deactivate Exception: {Env.DebugInfo}-> {Uid}-> {Script?.GetType()}\n{err}");
            }
            VLEnvironment.NodeActionCallback?.Invoke(Env, this, VLEnvironment.ENodeActionType.DeactivateEventNode);
        }


        public void EventTrigger()
        {
            Log.Verbose?.Write($"event trigger {Uid}|{Env.Uid}", "VLLogic");
            VLEnvironment.NodeActionCallback?.Invoke(Env, this, VLEnvironment.ENodeActionType.TriggerEventNode);
            try
            {
                if (ExecuteNext() > 0 && IsTriggerOnce)
                {
                    Deactivate();
                }
            }
            catch (Exception err)
            {
                Log.Error?.Write($"EventNode Trigger Exception: {Env.DebugInfo}-> {Uid}-> {Script?.GetType()}\n{err}");
            }
        }

        public override void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 5)
            {
                var typeName = reader.ReadString();
                Script = (VLBaseEventScript)TypeAssistant.New(typeName);
                Script.Env = Env;
                Script.Node = this;
                BytesPack.Unpack(ref Script, ref reader);
            }
            else if (key == 6)
            {
                IsTriggerOnce = reader.Read<bool>();
            }
            else
            {
                base.Z_BytesPackRead(key, ref reader);
            }
        }

        public override void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            base.Z_BytesPackWrite(ref key, ref writer);
            key.Push(ref writer, 5);
            writer.Push(TypeAssistant.GetTypeName(Script.GetType()));
            BytesPack.Pack(Script, ref writer);
            key.Push(ref writer, 6);
            writer.Push(IsTriggerOnce);
        }
    }
}
