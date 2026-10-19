// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Collections.Generic;
using System.Linq;
using FLib.WorldCores;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FLib.Unity
{
    [Serializable]
    public class UnityExternalReferenceStorer : ExternalReferenceStorer
    {
        public Object[] Objects = Array.Empty<Object>();
        public override object this[int index] { get => Objects[index]; set => Objects[index] = (Object)value; }

        /// <summary>
        /// 
        /// </summary>
        public override void SetArraySize(int newSize)
        {
            if (newSize == 0)
                Objects = Array.Empty<Object>();
            else
                Array.Resize(ref Objects, newSize);
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetArraySize() => Objects.Length;

        /// <summary>
        /// 
        /// </summary>
        public void MapToTarget(Transform root, Transform originalRoot)
        {
            var paths = new Stack<string>(8);
            for (var i = 0; i < Objects.Length; i++)
            {
                ref var obj = ref Objects[i];
                try
                {
                    if (obj is GameObject go && go.transform.root == originalRoot)
                    {
                        var path = GetPath(go.transform);
                        obj = root.Find(path);
                    }
                    else if (obj is Component comp && comp.transform.root == originalRoot)
                    {
                        var path = GetPath(comp.transform);
                        var targetTf = root.Find(path);
                        var targetIndex = comp.GetComponentIndex();
                        obj = targetTf.gameObject.GetComponentAtIndex(targetIndex);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"map error: {root.name}>{obj.name}", e);
                }
            }
            return;

            string GetPath(Transform tf)
            {
                paths.Clear();
                while (tf != null && tf.parent != null)
                {
                    paths.Push(tf.name);
                    tf = tf.parent;
                }
                return string.Join('/', paths);
            }
        }

        public UnityExternalReferenceStorer Clone() => new() { Objects = Objects.ToArray(), FreeIndexes = FreeIndexes.ToList() };
    }
}
