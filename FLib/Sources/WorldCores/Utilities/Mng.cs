// ==================== qcbf@qq.com | 2026-01-10 ====================

namespace FLib.WorldCores
{
    public struct Mng<T> : IComponentDestroy, IComponentAwake
    {
        private static FixedIndexList<T> _objects;

        private int _index;

        public ref T Val => ref _objects.GetRef(_index - 1);

        public Mng(in T v)
        {
            _index = _objects.Add(v) + 1;
        }

        public void ComponentBegin(WorldCore world, Entity entity)
        {
            if (_index == 0)
                _index = _objects.Add(default) + 1;
        }

        public void ComponentEnd(WorldCore world, Entity entity)
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