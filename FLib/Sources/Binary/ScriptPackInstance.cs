// ==================== qcbf@qq.com | 2026-03-27 ====================

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FLib
{
    public struct ScriptPackInstance : IJson5Serializable, IJson5Deserializable, IBytesSerializable, IBytesPackable, IEquatable<ScriptPackInstance>, IScriptPackable
    {
        private const string ScriptTypeJsonKey = "$type";
        public IBytesPackable Instance;
        public readonly bool IsEmpty => Instance == null;
        public readonly Type ScriptType => Instance?.GetType();

        public readonly Type ScriptBaseType => typeof(IBytesPackable);

        public ScriptPackInstance(IBytesPackable instance) => Instance = instance;
        public void SetInstance(IBytesPackable instance) => Instance = instance;
        public IBytesPackable CreateInstance() => Instance;

        public static bool operator ==(in ScriptPackInstance left, in ScriptPackInstance right) => left.Equals(right);
        public static bool operator !=(in ScriptPackInstance left, in ScriptPackInstance right) => !(left == right);
        public override string ToString() => Instance?.ToString() ?? "null";
        public readonly bool Equals(ScriptPackInstance other) => EqualityComparer<object>.Default.Equals(Instance, other.Instance);
        public override bool Equals(object obj) => obj is ScriptPackInstance other && Equals(other);
        public override int GetHashCode() => EqualityComparer<object>.Default.GetHashCode(Instance);

        #region serialization

        public readonly bool JsonSerialize(StringBuilder jsonText, object serializeObject, object customData, int indent, Json5SerializeOptionData opData)
        {
            JsonSerializeImpl(jsonText, Instance, 0);
            return true;
        }

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
            => JsonDeserializeImpl(ref nodes, out Instance!, null);

        public readonly void Z_BytesWrite(ref BytesWriter writer)
            => writer.PushScript(Instance);

        public void Z_BytesRead(ref BytesReader reader)
            => Instance = reader.ReadScript();

        public readonly void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            Z_BytesWrite(ref writer);
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
                Z_BytesRead(ref reader);
        }

        internal static Json5CustomDeserializeResult JsonDeserializeImpl(ref Json5SyntaxNodes nodes, out IBytesPackable instance, string ns)
        {
            if (!Json5SyntaxNodesReader.TryCreate(ref nodes, out var node, out var reader))
            {
                instance = null;
                return false;
            }

            if (!node.ContentSpan.Equals(ScriptTypeJsonKey, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException(node.ContentCopyString);
            var typeName = reader.Read(ref nodes).ContentCopyString;
            if (ns != null)
            {
                typeName = StringFLibUtility.ReleaseStrBufAndResult(StringFLibUtility.GetStrBuf(typeName.Length + ns.Length + 1)
                    .Append(ns).Append('.').Append(typeName));
            }

            nodes.Nodes[--nodes.Position].Token = EJson5Token.ObjectOpen;
            --reader.BracketOpenCount; // 因为上面 Token = EJson5Token.ObjectOpen 强制再open了一次, 为了让下面的json正确走入ObjectOpen解析
            var o = nodes.To(TypeAssistant.GetType(typeName));
            System.Diagnostics.Debug.Assert(o is IBytesPackable, $"TypeAssistant.GetType({typeName}) return {o.GetType()}, not IBytesPackable");
            instance = (IBytesPackable)o;
            reader.Close(ref nodes);
            return true;
        }

        internal static void JsonSerializeImpl(StringBuilder strbuf, IBytesPackable instance, int nsLen)
        {
            if (instance == null)
            {
                strbuf.Append("{}");
            }
            else
            {
                var startLen = strbuf.Length;
                Json5Serializer.PushDictKey(ScriptTypeJsonKey, strbuf, 0, default);
                var typeName = TypeAssistant.GetTypeName(instance.GetType());
                if (nsLen > 0)
                    typeName = typeName[(nsLen + 1)..];
                Json5Serializer.PushValue(typeName, strbuf, 0, default);
                strbuf.Append(',').Append(' ');
                var pos = strbuf.Length;
                Json5Serializer.PushValue(instance, strbuf, 0, default);
                for (var i = pos; i > startLen; i--)
                    strbuf[i] = strbuf[i - 1];
                strbuf[startLen] = '{';
            }
        }

        #endregion
    }

    public struct ScriptPackInstance<T> : IJson5Serializable, IJson5Deserializable, IBytesSerializable, IBytesPackable, IScriptPackable, IEquatable<ScriptPackInstance<T>>
        where T : IBytesPackable
    {
        public T Instance;

        public ScriptPackInstance(T instance) => Instance = instance;

        public readonly bool IsEmpty => Instance == null;
        public readonly Type ScriptType => Instance?.GetType();

        public readonly Type ScriptBaseType => typeof(T);
        public void SetInstance(IBytesPackable instance) => Instance = (T)instance;
        public IBytesPackable CreateInstance() => Instance;

        public override string ToString() => Instance?.ToString() ?? "null";
        public static implicit operator T(in ScriptPackInstance<T> v) => v.Instance;
        public static implicit operator ScriptPackInstance<T>(T v) => new(v);
        public bool Equals(ScriptPackInstance<T> other) => EqualityComparer<T>.Default.Equals(Instance, other.Instance);
        public override bool Equals(object obj) => obj is ScriptPackInstance<T> other && Equals(other);
        public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Instance);

        public static bool operator ==(in ScriptPackInstance<T> left, in ScriptPackInstance<T> right)
            => left.Equals(right);

        public static bool operator !=(in ScriptPackInstance<T> left, in ScriptPackInstance<T> right)
            => !(left == right);

        #region serialization

        public readonly bool JsonSerialize(StringBuilder jsonText, object serializeObject, object customData, int indent, Json5SerializeOptionData opData)
        {
            ScriptPackInstance.JsonSerializeImpl(jsonText, Instance, typeof(T).Namespace?.Length ?? 0);
            return true;
        }

        public Json5CustomDeserializeResult JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options)
        {
            var r = ScriptPackInstance.JsonDeserializeImpl(ref nodes, out var inst, typeof(T).Namespace);
            if (r.IsHooked)
                Instance = (T)inst;
            return r;
        }

        public readonly void Z_BytesWrite(ref BytesWriter writer)
            => writer.PushScript(Instance);

        public void Z_BytesRead(ref BytesReader reader)
            => Instance = (T)reader.ReadScript();

        public readonly void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            Z_BytesWrite(ref writer);
        }

        public void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
                Z_BytesRead(ref reader);
        }

        #endregion
    }
}