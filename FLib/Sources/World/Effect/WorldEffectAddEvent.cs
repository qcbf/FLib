// =================================================={By Qcbf|qcbf@qq.com|2024-11-05}==================================================


namespace FLib.Worlds
{
    public readonly struct WorldEffectAddEvent
    {
        public readonly long Key;
        public readonly WorldEffect EffectConfig;
        public readonly WorldEntity CreateBy;
        public readonly int AddCount;

        public WorldEffectAddEvent(long key, WorldEffect effectConfig, in WorldEntity createBy, int addCount)
        {
            Key = key;
            CreateBy = createBy;
            AddCount = addCount;
            EffectConfig = effectConfig;
        }
    }
}
