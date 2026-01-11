// ==================== qcbf@qq.com |2026-01-03 ====================

// ReSharper disable StaticMemberInGenericType

namespace FLib.WorldCores
{
    public static class ComponentGenericMap<T>
    {
        public static ComponentMeta Meta { get; internal set; }
        public static ComponentInfo Info;
        public static IncrementId Id => Meta.Id;
        public static ushort Size => Meta.Size;
        public static bool IsEmpty => Meta.Id.IsEmpty;

        internal static ComponentMeta Init(ComponentMeta meta)
        {
            Info = ComponentRegistry.GetInfo(meta);
            Meta = meta;
            return Meta;
        }
    }
}