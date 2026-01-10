//==================={By Qcbf|qcbf@qq.com|5/30/2021 11:19:40 AM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FLib.Worlds
{
    public class VLEnvironment : IBytesPackable
    {
        public static Action<VLEnvironment, VLNode, ENodeActionType> NodeActionCallback;
        public static Dictionary<long, List<VLEnvironment>> AllVLEnvironments = new();

        public VLEnvironment Parent;
        public Func<string, object> ScriptLoader;
        public object RuntimeData;
        public string DebugInfo;
        public Action<string, string, string, string> CustomEventDispatcher;
        public uint[] StartupNodeUids;
        public long Uid;
        public VLDynamicFields Variables = new();
        public Dictionary<uint, VLNode> Nodes = new();
#if UNITY_PROJ
        public VLUnityObjectStorer UnityObjectStorer;
#endif

        public enum ENodeActionType
        {
            ActivateEventNode,
            DeactivateEventNode,
            TriggerEventNode,
            ExecuteLogicNode,
            ExecuteTemplateNode,
        }

        public VLNode GetNode(uint uid, bool isThrowOnError = true)
        {
            if (!Nodes.TryGetValue(uid, out var node) && isThrowOnError) throw new Exception("not found node: " + uid);
            return node;
        }

        public void Startup()
        {
            if (Uid != 0)
            {
                if (!AllVLEnvironments.TryGetValue(Uid, out var list))
                {
                    AllVLEnvironments.Add(Uid, list = new List<VLEnvironment>());
                }
                list.Add(this);
            }
            if (StartupNodeUids != null)
            {
                foreach (var nodeUid in StartupNodeUids)
                {
                    Nodes.TryGetValue(nodeUid, out var node);
                    Log.AssertNotNull(node)?.Write("not found startup node:" + nodeUid);
                    node.Execute();
                }
            }
        }

        public void DeactivateEventNodes(IList<VLEventNode> deactivateNodes = null)
        {
            foreach (var item in Nodes)
            {
                if (item.Value is VLEventNode eNode && eNode.IsActivated)
                {
                    eNode.Deactivate();
                    deactivateNodes?.Add(eNode);
                }
                // else if (item.Value is VLTemplateNode tNode)
                // {
                //     tNode.TemplateVLEnv?.DeactivateEventNodes(deactivateNodes);
                // }
            }
            if (Uid != 0)
            {
                AllVLEnvironments[Uid].Remove(this);
            }
        }

        public void ActivateEventNodes(IList<VLEventNode> activateNodes)
        {
            if (Uid != 0)
            {
                if (!AllVLEnvironments.TryGetValue(Uid, out var list))
                {
                    AllVLEnvironments.Add(Uid, list = new List<VLEnvironment>());
                }
                list.Add(this);
            }
            foreach (var item in activateNodes)
            {
                item.Activate();
            }
        }

        public void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.Push(Uid);

            key.Push(ref writer, 2);
            BytesPack.Pack(Variables, ref writer);

            key.Push(ref writer, 3);
            var hasLeftNodeUids = new HashSet<uint>();
            writer.PushLength(Nodes.Count);
            foreach (var node in Nodes)
            {
                writer.Push(node.Key);
                writer.Push(TypeAssistant.GetTypeName(node.Value.GetType()));
                BytesPack.Pack(node.Value, ref writer);
                foreach (var line in node.Value.Lines)
                    hasLeftNodeUids.Add(line.RightNodeUid);
            }

            key.Push(ref writer, 4);
            writer.Push(Nodes.Where(v => !hasLeftNodeUids.Contains(v.Key)).Select(v => v.Key).ToArray());
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            switch (key)
            {
                case 1: reader.Read(ref Uid); break;
                case 2:
                    BytesPack.Unpack(ref Variables, ref reader);
                    foreach (var variable in Variables.Values)
                        ((VLValueBase)variable.Value.Value).Env = this;
                    break;
                case 4:
                    reader.Read(ref StartupNodeUids);
                    break;
                case 3:
                    var count = reader.ReadLength();
                    Nodes = new Dictionary<uint, VLNode>(count);
                    for (var i = 0; i < count; i++)
                    {
                        var k = reader.Read<uint>();
                        var typeName = reader.ReadString();
                        var node = (VLNode)TypeAssistant.New(typeName);
                        node.Env = this;
                        Nodes.Add(k, node);
                        BytesPack.Unpack(ref node, ref reader);
                        if (!node.IsLateInitialization)
                            node.Initialize();
                    }
                    break;
            }
        }
    }
}
