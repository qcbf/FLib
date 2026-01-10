// =================================================={By Qcbf|qcbf@qq.com|2024-11-06}==================================================

using System.Threading;

namespace FLib.Worlds
{
    public struct WorldSyncUidGenerator
    {
        public readonly ushort NetId;
        public int IncrementId;

        public readonly override string ToString() => $"{NetId},{IncrementId}";

        public WorldSyncUidGenerator(ushort netId)
        {
            Log.Assert(netId > 0)?.Write("netid must greater zero");
            NetId = netId;
            IncrementId = 0;
        }

        public int Gen() => Gen(NetId, Interlocked.Increment(ref IncrementId));
        public static int Gen(ushort netId, int increment) => netId << 20 | (increment & 0xfffff);

        public static void BytesWrite(ref BytesWriter writer, int uid)
        {
            writer.PushVInt(uid >> 20);
            writer.PushVInt(uid & 0xfffff);
        }

        public static int BytesRead(ref BytesReader reader)
        {
            var netId = (ushort)reader.ReadVInt();
            var increment = (ushort)reader.ReadVInt();
            return Gen(netId, increment);
        }
    }
}
