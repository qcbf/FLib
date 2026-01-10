// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Runtime.CompilerServices;
using FLib.Worlds;

namespace FLib.Worlds
{
    [BytesPackGenHoldKey(2)]
    public class TimeLogicRuntime : IBytesPackable
    {
        public object UserData;

        /// <summary>
        /// obj is TimeLogicTrack or TimeLogicClip
        /// 返回 false 表示阻止执行
        /// </summary>
        public Func<object, bool> ExecuteVerifyHandler;

        public TimeLogicTrack[] Tracks = Array.Empty<TimeLogicTrack>();
        public bool IsLoop = true;
        public int EndFrame;
        public string Name;
        private FNum _currentFrame;
        private FNum _frameDelta;
        private byte _frameRate = 30;
        public bool IsEndFrameOver;
        public byte FrameRate { get => _frameRate; set => _frameDelta = FNum.One / (_frameRate = value); }
        public int FrameCount => EndFrame + 1;
        public ExternalReferenceStorer ExternalReferences;

        public int CurrentFrame
        {
            get => (int)_currentFrame;
            set
            {
                if (_currentFrame == value)
                    return;
                _currentFrame = value;
                UpdateCurrentFrame();
            }
        }

        public override string ToString() => $"{Name},{CurrentFrame}";

        public TimeLogicRuntime()
        {
            _frameDelta = FNum.One / _frameRate;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop(bool isResetFrame = true)
        {
            if (isResetFrame)
                _currentFrame = 0;
            foreach (var track in Tracks)
            {
                try
                {
                    track.Stop();
                }
                catch (Exception e)
                {
                    Log.Error?.Write($"{Name} {CommentAttribute.TryGetLabel(track?.GetType())} {e}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateNextFrame(FNum frameDelta)
        {
            var lastFrame = (int)_currentFrame;
            _currentFrame += frameDelta / _frameDelta;
            if (lastFrame == (int)_currentFrame)
                return;
            if (CurrentFrame > EndFrame)
            {
                if (IsLoop)
                    Stop();
                else
                    CurrentFrame = EndFrame;
                UpdateCurrentFrame();
                IsEndFrameOver = true;
            }
            else
            {
                UpdateCurrentFrame();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateCurrentFrame()
        {
            IsEndFrameOver = false;
            foreach (var track in Tracks)
            {
                if (!track.IsDisable && ExecuteVerifyHandler?.Invoke(track) != false)
                    track.Update();
            }
            foreach (var track in Tracks)
            {
                if (!track.IsDisable && ExecuteVerifyHandler?.Invoke(track) != false)
                    track.LateUpdate();
            }
        }

        public virtual void Z_BytesPackWrite(ref BytesPack.KeyHelper key, ref BytesWriter writer)
        {
            key.Push(ref writer, 1);
            writer.PushVInt(0);
            writer.Push(FrameRate);
            writer.Push(IsLoop);
            writer.PushVInt(EndFrame);
            writer.PushScript(Tracks);
        }

        public virtual void Z_BytesPackRead(int key, ref BytesReader reader)
        {
            if (key == 1)
            {
                reader.ReadVInt();
                FrameRate = reader.Read<byte>();
                IsLoop = reader.Read<bool>();
                EndFrame = (int)reader.ReadVInt();
                Tracks = reader.ReadScripts<TimeLogicTrack>();
                foreach (var track in Tracks)
                    track.Runtime = this;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EntityTimeLogicRuntime : TimeLogicRuntime
    {
        public WorldEntity Entity;
    }
}
