// ==================== qcbf@qq.com | 2025-07-01 ====================

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FLib;
using FLib.Unity;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace FLib.Unity
{
    public class GpuAnimBaker : MonoBehaviour
    {
        public GameObject AnimRoot;
        public GpuAnimRenderer Renderer;
        public SkinnedMeshRenderer Skinned;
        public int TexWidth;
        public ClipData[] ClipDatas;
        public Transform[] HoldBones;

        [Serializable]
        public class ClipData
        {
            public AnimationClip Clip;
            public int FrameRate;
            public bool IsLoop;
        }

        [MethodButton]
        public void Bake()
        {
            Renderer.HoldBoneNames = HoldBones.Select(v => v.name).ToArray();
            Renderer.AnimClips = new GpuAnimClip[ClipDatas.Length];

            // root transform matrix
            var rootTf = Skinned.transform;
            Renderer.RootTransform = float4x4.TRS(rootTf.localPosition, rootTf.localRotation, rootTf.localScale);
            while (rootTf.parent != null)
            {
                rootTf = rootTf.parent;
                Renderer.RootTransform = Matrix4x4.TRS(rootTf.localPosition, rootTf.localRotation, rootTf.localScale) * (Matrix4x4)Renderer.RootTransform;
            }

            var animClipFrameVerticesCount = 0;
            var vertexCount = Skinned.sharedMesh.vertexCount;
            var validClipCount = 0;
            for (var i = ClipDatas.Length - 1; i >= 0; i--)
            {
                try
                {
                    var bakeClip = ClipDatas[i];
                    if (bakeClip.Clip == null)
                        continue;
                    ++validClipCount;
                    ref var renderClip = ref Renderer.AnimClips[i];
                    var frameCount = (int)(bakeClip.Clip.length * bakeClip.FrameRate);
                    animClipFrameVerticesCount += frameCount * vertexCount;
                    renderClip.FrameCount = frameCount;
                    renderClip.IsLoop = bakeClip.IsLoop;
                    renderClip.FrameRate = bakeClip.FrameRate;
                }
                catch (Exception e)
                {
                    throw new Exception($"bake failure clip[{i}]", e);
                }
            }

            var size = new Vector2Int(TexWidth, 0);
            if (size.x <= 0)
                size.x = (int)((int)math.ceil(math.sqrt(animClipFrameVerticesCount)) / 4f) * 4;
            size.y = (int)math.ceil(animClipFrameVerticesCount / (float)size.x / 4f) * 4;
            var animTex = new Texture2D(size.x, size.y, TextureFormat.RGBAFloat, false, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point };
            var tempMesh = new Mesh();
            var vertexIndex = 0;
            for (var i = 0; i < ClipDatas.Length; i++)
            {
                var bakeClip = ClipDatas[i];
                ref var renderClip = ref Renderer.AnimClips[i];
                var clip = bakeClip.Clip;
                if (clip == null)
                    continue;
                renderClip.VertexIndex = vertexIndex;

                // bone
                renderClip.Bones = new GpuAnimClipBone[HoldBones.Length];
                for (var j = 0; j < HoldBones.Length; j++)
                    renderClip.Bones[j].TRFrames = new float2x3[renderClip.FrameCount];

                // vertex
                var tempVertices = new List<Vector3>();
                for (var frame = 0; frame < renderClip.FrameCount; frame++)
                {
                    clip.SampleAnimation(AnimRoot, frame / (float)renderClip.FrameRate);

                    // bone
                    if (renderClip.Bones != null)
                    {
                        for (var j = 0; j < renderClip.Bones.Length; j++)
                        {
                            var pos = HoldBones[j].position;
                            var rot = HoldBones[j].eulerAngles;
                            renderClip.Bones[j].TRFrames[frame] = new float2x3(pos.x, pos.y, pos.z, rot.x, rot.y, rot.z);
                        }
                    }

                    Skinned.BakeMesh(tempMesh);
                    tempVertices.Clear();
                    tempMesh.GetVertices(tempVertices);
                    var frameBlockIndex = renderClip.VertexIndex + vertexCount * frame;
                    for (var j = 0; j < tempVertices.Count; j++)
                    {
                        var v = frameBlockIndex + j;
                        var x = v % size.x;
                        var y = v / size.x;
                        animTex.SetPixel(x, y, new Color(tempVertices[j].x, tempVertices[j].y, tempVertices[j].z, 0));
                    }
                }
                clip.SampleAnimation(AnimRoot, 0);
                vertexIndex = renderClip.VertexIndex + vertexCount * renderClip.FrameCount;
                Log.Info?.Write($"Bake[{clip.name}] Frames:{renderClip.FrameCount} Begin:{renderClip.VertexIndex}|{renderClip.VertexIndex % size.x},{renderClip.VertexIndex / size.x} End:{vertexIndex}|{vertexIndex % size.x},{vertexIndex / size.x}");
            }
            ClipDatas[0].Clip.SampleAnimation(AnimRoot, 0);
            animTex.Apply();
            AssetDatabase.CreateAsset(animTex, FIO.PathRename(GetPrefabAssetPath(gameObject), "AnimTex.asset", true, false));

            Log.Info?.Write($"Bake Complete Size:{size}. ClipCount:<color=#ff0000><b>{validClipCount}</b></color>, VertexCount:{animClipFrameVerticesCount}");
            DestroyImmediate(tempMesh);
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(Renderer);
            AssetDatabase.Refresh();

            Renderer.AnimTex = animTex;
            Renderer.RenderMesh = Skinned.sharedMesh;
        }

        public static string GetPrefabAssetPath(GameObject target)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && target.scene == stage.scene)
                return stage.assetPath;
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);
            if (!string.IsNullOrEmpty(path))
                return path;
            return AssetDatabase.GetAssetPath(target);
        }
    }
}

#endif
