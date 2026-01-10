// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;

namespace FLib.Worlds
{
    [BytesPackGenHoldKey(2), Comment("基础片段")]
    public class TimeLogicClip : IBytesPackable
    {
        public TimeLogicTrack Track;
        [Comment("名称")] public string Name;
        [Comment("是否禁用")] public bool IsDisable;

        public virtual int BeginFrame { get; set; }
        public virtual int EndFrame { get; set; }
        public TimeLogicRuntime Runtime => Track.Runtime;
        public int CurrentFrame => Runtime.CurrentFrame;
        public int CurrentClipFrame => Runtime.CurrentFrame - BeginFrame;
        public FNum CurrentClipTime => (FNum)CurrentClipFrame / Runtime.FrameRate;
        public virtual int FrameCount => EndFrame - BeginFrame + 1;

        /// <summary>
        /// 
        /// </summary>
        public virtual void Begin()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void End()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void LateUpdate()
        {
        }

        public T GetExternalReference<T>(in ExternalReferenceField<T> field) where T : class => field.Index < 0 || field.Index >= Runtime.ExternalReferences.GetArraySize() ? null : Runtime.ExternalReferences[field.Index] as T;
        public bool TryGetExternalReference<T>(in ExternalReferenceField<T> field, out T val) where T : class => (val = GetExternalReference(field)) != null;

        public T GetSelfOrExternalReference<T>(in ExternalReferenceField<T> target) where T : class
        {
            if (target.Index >= 0)
                return GetExternalReference(target);
            return Runtime.UserData as T;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool CheckFrame(int frame) => BeginFrame <= frame && EndFrame >= frame;

        /// <summary>
        /// 
        /// </summary>
        public virtual bool CheckFrame() => CheckFrame(CurrentFrame);

        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.Push(Name);
            writer.Push(IsDisable);
            writer.PushVInt(BeginFrame);
            writer.PushVInt(EndFrame);
        }

        public virtual void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
            {
                Name = reader.ReadString();
                IsDisable = reader.Read<bool>();
                BeginFrame = (int)reader.ReadVInt();
                EndFrame = (int)reader.ReadVInt();
            }
        }
    }
}
