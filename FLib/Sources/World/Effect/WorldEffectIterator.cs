// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================

using System.Collections;
using System.Collections.Generic;

namespace FLib.Worlds
{
    public struct WorldEffectIterator : IEnumerator<WorldEffectContext>, IEnumerable<WorldEffectContext>
    {
        public readonly WorldEffectContext First;

        readonly object IEnumerator.Current => Current;
        public WorldEffectContext Current { get; set; }

        public WorldEffectIterator(WorldEffectContext first)
        {
            First = first;
            Current = null;
        }

        public bool MoveNext()
        {
            if (Current == null)
                Current = First;
            else
                Current = Current.Next;
            return Current != null;
        }

        public void Reset() => Current = null;
        public void Dispose() => Reset();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public readonly IEnumerator<WorldEffectContext> GetEnumerator() => new WorldEffectIterator(First);
    }
}
