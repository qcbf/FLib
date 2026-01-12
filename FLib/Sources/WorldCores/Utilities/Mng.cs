// ==================== qcbf@qq.com | 2026-01-10 ====================

namespace FLib.WorldCores
{
    public struct Mng<T> : IComponentDestroy, IComponentAwake
    {
        /// <summary>
        /// 略微感觉做法有点糙, 但又没想出是否要单独写个分页对象储存池,感觉好像又没太大必要, 暂时先这样实现
        /// </summary>
        private static FixedIndexList<T> _objects;

        private int _index;

        public ref T Val => ref _objects.GetRef(_index - 1);

        public override string ToString() => Val.ToString();

        public Mng(in T v)
        {
            _index = _objects.Add(v) + 1;
        }

        public void ComponentAwake(WorldCore world, Entity entity)
        {
            if (_index == 0)
                _index = _objects.Add(default) + 1;
        }

        public void ComponentDestroy(WorldCore world, Entity entity)
        {
            if (_index == 0) return;
            _objects.RemoveAt(_index - 1);
            _index = 0;
        }

        public static implicit operator T(Mng<T> mng) => _objects[mng._index - 1];
    }

    public static class Mng
    {
        public static Mng<T> T<T>(in T v) => new(v);
    }
}