// ==================== qcbf@qq.com |2026-01-01 ====================

using System;

namespace FLib.WorldCores
{
    public readonly struct ComponentMeta
    {
        public readonly IncrementId Id;
        public readonly ushort Size;

        public override string ToString() => $"{Id}-{FIO.FormatSize(Size)}-{GetTypeName(Type)}";
        public Type Type => ComponentRegistry.GetType(this);

        internal ComponentMeta(IncrementId id, ushort size)
        {
            Id = id;
            Size = size;
        }

        public static implicit operator Type(in ComponentMeta meta) => meta.Type;
        public static implicit operator IncrementId(in ComponentMeta meta) => meta.Id;

        /// <summary>
        ///
        /// </summary>
        public static string GetTypeName(Type t)
        {
            var tName = t.Name;
            var strbuf = StringFLibUtility.GetStrBuf(tName.Length);
            strbuf.Append(tName);
            var parentType = t.DeclaringType;
            while (parentType != null)
            {
                tName = parentType.Name;
                strbuf.EnsureCapacity(strbuf.Length + tName.Length);
                strbuf.Insert(0, '.').Insert(0, tName);
                parentType = parentType.DeclaringType;
            }

            return StringFLibUtility.ReleaseStrBufAndResult(strbuf);
        }
    }
}