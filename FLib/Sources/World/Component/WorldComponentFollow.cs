//=================================================={By Qcbf|qcbf@qq.com|11/19/2024 9:17:26 PM}==================================================

using FLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FLib.Worlds
{
    public class WorldComponentFollow
    {
        public static ReadOnlyDictionary<ushort, FollowerData[]> AllComponentFollowerDatas;

        public readonly Dictionary<WorldComponentHandle, List<WorldComponentHandle>> Follower = new(AllComponentFollowerDatas.Count);


        public struct FollowerData
        {
            public Func<WorldEntity, WorldComponentHandleEx, Action<WorldComponentHandle>, bool> FollowerAddHook;
            public ushort CompTypeId;
        }

        public static WorldComponentFollow Create(WorldBase world)
        {
            var f = new WorldComponentFollow();
            world.ListenEvent<WorldAddComponentEvent>(f.OnComponentAddEvent);
            world.ListenEvent<WorldRemoveComponentEvent>(f.OnComponentRemoveEvent);
            return f;
        }

        private void OnComponentRemoveEvent(object dispatcher, in WorldRemoveComponentEvent e)
        {
            if (!Follower.Remove(e.CompHandle, out var followers))
                return;
            if (e.Entity.IsEmpty) return;
            foreach (var t in followers)
                e.Entity.Remove(t);
        }

        private void OnComponentAddEvent(object dispatcher, in WorldAddComponentEvent e)
        {
            if (!AllComponentFollowerDatas.TryGetValue(e.CompHandle.TypeId, out var followerDatas))
                return;
            var follower = new List<WorldComponentHandle>(followerDatas.Length);
            Follower.Add(e.CompHandle, follower);
            foreach (var data in followerDatas)
            {
                if (data.FollowerAddHook?.Invoke(e.Entity, e.CompHandle, follower.Add) == true) continue;
                var followerCompHandle = e.Entity.Add(data.CompTypeId);
                if (!followerCompHandle.IsEmpty)
                    follower.Add(followerCompHandle);
            }
            follower.TrimExcess();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class WorldComponentFollowAttribute : Attribute
    {
        public readonly Type Target;

        /// <summary>
        /// <code>private static bool FollowerAddHook(WorldEntity entity, WorldComponentHandleEx comp, Action&lt;WorldComponentHandle&gt; addHandler) => false;</code>
        /// </summary>
        public string FollowerAddHook;

        public WorldComponentFollowAttribute(Type target) => Target = target;
    }
}
