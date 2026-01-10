// =================================================={By Qcbf|qcbf@qq.com|2024-11-08}==================================================

using System.Threading.Tasks;

namespace FLib.Worlds
{
    public class WorldLocalSyncer : WorldSyncer
    {
        public WorldSyncer[] Links;

        public WorldLocalSyncer(WorldBase world, ushort netId) : base(world, netId)
        {
        }

        public override void SendCommandBuffer(BytesReader reader)
        {
            foreach (var item in Links)
                item.ReceiveCommandBuffer(reader);
        }
    }
}
