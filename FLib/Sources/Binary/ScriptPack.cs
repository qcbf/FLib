//==================={By Qcbf|qcbf@qq.com|8/19/2021 6:59:03 PM}===================

using FLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FLib
{
    [BytesPackGenHoldKey(1)]
    public class ScriptPack : IBytesPackable, IJson5Deserializable
    {
        public Type Type;
        public byte[] Bytes;
        public bool IsValid => Type != null;
        public object UserInstance;
        public virtual Type BaseType => typeof(object);

        public ScriptPack()
        {
        }

        public ScriptPack(object script)
        {
            Set(script);
        }

        /// <summary>
        ///
        /// </summary>
        public virtual void Set(object script)
        {
            Type = script.GetType();
            UserInstance = script;
            if (script is not IBytesPackable bytesPackable)
                return;
            var writer = new BytesWriter { Allocator = BytesWriter.PoolAllocator };
            try
            {
                BytesPack.Pack(bytesPackable, ref writer);
                Bytes = writer.Span.ToArray();
            }
            finally
            {
                writer.TryReleasePoolAllocator();
            }
        }

        /// <summary>
        /// 从当前数据创建对象
        /// </summary>
        public object Create(bool isThrowOnException = true)
        {
            if (Type == null)
                return isThrowOnException ? throw new Exception("not found data") : null;

            var script = TypeAssistant.New(Type);
            if (script is IBytesPackable bytesPackable)
                BytesPack.Unpack(ref bytesPackable, Bytes);
            return script;
        }

        /// <summary>
        /// 从当前数据创建对象
        /// </summary>
        public T Create<T>(bool isThrowOnException = true) where T : new()
        {
            if (Type == null)
                return isThrowOnException ? throw new Exception("not found data") : default;

            var script = new T();
            if (script is IBytesPackable bytesPackable)
                BytesPack.Unpack(ref bytesPackable, Bytes);
            return script;
        }

        /// <summary>
        /// 从当前数据创建对象
        /// </summary>
        public void CopyTo<T>(ref T v) where T : IBytesPackable, new()
        {
            if (Type == null) return;
            BytesPack.Unpack(ref v, Bytes);
        }

        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 0);
            writer.Push(Type == null ? string.Empty : GetScriptTypeName(Type));
            writer.Push(Bytes);
        }

        public virtual void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 0)
            {
                var typeName = reader.ReadString();
                Bytes = reader.ReadArray<byte>();
                if (typeName.Length > 0)
                    Type = GetScriptType(typeName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Json5CustomDeserializeResult IJson5Deserializable.JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            var (typeName, custom) = JsonDeserializeTypeName(ref nodes);
            Type = GetScriptType(typeName) ?? throw new Exception($"not found type: {typeName}");
            Set(Json5Deserializer.ToObject(ref nodes, Type, options));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public static (string Name, string Custom) JsonDeserializeTypeName(ref Json5SyntaxNodes nodes)
        {
            var scriptName = string.Empty;
            var custom = string.Empty;

            for (var i = 0; i < nodes.Count; i++)
            {
                ref var node = ref nodes[i];
                if (node.ContentSpan.Length > 0 && node.ContentSpan[0] == '$')
                {
                    node.Token = EJson5Token.None;
                    node = ref nodes[++i];
                    if (node.Token == EJson5Token.Value)
                    {
                        scriptName = node.ContentCopyString;
                        node.Token = EJson5Token.None;
                    }
                    else if (node.Token == EJson5Token.ArrayOpen)
                    {
                        node = ref nodes[++i];
                        scriptName = node.ContentCopyString;
                        node.Token = EJson5Token.None;
                        node = ref nodes[i + 1];
                        custom = node.ContentCopyString;
                        node.Token = EJson5Token.None;
                    }
                    else
                    {
                        throw new NotSupportedException(node.ToString());
                    }
                    break;
                }
            }
            return (FormatConfigTypeName(scriptName), custom);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string GetScriptTypeName(Type t)
        {
            var name = TypeAssistant.GetTypeName(t);
            if (BaseType != typeof(object) && BaseType.Namespace != null)
                name = name[(BaseType.Namespace.Length + 1)..];
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Type GetScriptType(string typeName)
        {
            if (BaseType != typeof(object))
                typeName = StringFLibUtility.ReleaseStrBufAndResult(StringFLibUtility.GetStrBuf().Append(BaseType.Namespace).Append('.').Append(typeName));
            var type = TypeAssistant.GetType(typeName, false, false);
            if (type == null)
                Log.Warn?.Write("not found type:" + typeName);
            return type;
        }

        /// <summary>
        /// Namespace_Script__NestedScript > Namespace.Script+NestedScript
        /// </summary>
        public static string FormatConfigTypeName(ReadOnlySpan<char> raw)
        {
            const char splitChar = '_';
            var strbuf = StringFLibUtility.GetStrBuf();
            for (var i = 0; i < raw.Length; i++)
            {
                if (raw[i] == splitChar)
                {
                    if (i + 1 < raw.Length && raw[i + 1] == splitChar)
                    {
                        strbuf.Append('+');
                        ++i;
                    }
                    else
                    {
                        strbuf.Append('.');
                    }
                }
                else
                {
                    strbuf.Append(raw[i]);
                }
            }

            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScriptPack<T> : ScriptPack, IConfigPostBuildProcessable
    {
        public new T UserInstance => (T)base.UserInstance;
        public override Type BaseType => typeof(T);
        public ScriptPack() { }
        public ScriptPack(in T script) : base(script) { }

        public new T Create(bool isThrowOnException = true) => (T)base.Create(isThrowOnException);

        //为了处理实例的类型里面有配置序列号期间额外处理，所以需要在最后的时候再写入一次最新的实例。
        void IConfigPostBuildProcessable.OnConfigPostBuildProcess(char sign, IConfigBuildTableContext context, IReadOnlyDictionary<Type, IConfigBuildTableContext> allTableContexts) => base.Set(base.UserInstance);
    }
}
