// ==================== qcbf@qq.com | 2025-09-11 ====================

using FLib;

namespace FLib
{
    [BytesPackGen]
    public partial struct BytesPackValue<T> where T : unmanaged
    {
        [BytesPackGenField] public T Value;
        public BytesPackValue(T value) => Value = value;
        public static implicit operator T(in BytesPackValue<T> v) => v.Value;
    }
}
