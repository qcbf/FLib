//==================={By Qcbf|qcbf@qq.com|7/14/2021 11:02:37 AM}===================

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FLib.Unity
{
    public static class AssetLoader
    {
        public const string BUNDLE_EXTENSION = ".b";
        public const string NON_BUNDLE_EXTENSION = ".nb";
        public const string INFO_FILE_NAME = "i" + NON_BUNDLE_EXTENSION;
        public const string INFO_ID_FILE_NAME = "ii" + NON_BUNDLE_EXTENSION;
        public const string GAME_RES_NAME = "GameRes";
        public const string BUNDLE_MAIN_ASSET_NAME = "*";
        public const string BUNDLE_SHADERS_ASSET_NAME = "SHADERS";

        public static AssetLoaderInfo Info;

        public static string CDN = "";
        public static string PersistentPath = Application.persistentDataPath;
        public static string PersistentAssetPath = Path.Combine(Application.persistentDataPath, GAME_RES_NAME);

        public static readonly Dictionary<string, AssetLoaded> ConstAssetLoadeds = new(16);
        public static readonly Dictionary2<string, AssetLoaded> AssetLoadeds = new(4096);
        public static ValueLinkedList<AssetLoading> AssetLoadings = new(4096);
        public static readonly Dictionary<string, int> AssetLoadingDict = new(4096);


        private static float _nextTryUnloadTime;
        private static uint _nextTryUnloadIdleCount;

        static AssetLoader()
        {
            FIO.CreateDirectory(PersistentAssetPath);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += Update;
#endif
        }

        /// <summary>
        ///
        /// </summary>
        public static void Update()
        {
            var t = Time.time;
            if (t >= _nextTryUnloadTime)
            {
                if (TryUnloadAll() == 0)
                    ++_nextTryUnloadIdleCount;
                else
                    _nextTryUnloadIdleCount = 0;
                _nextTryUnloadTime = t + 2 + _nextTryUnloadIdleCount * 2;
            }

            using var iterator = AssetLoadings.GetEnumerator();
            while (iterator.MoveNext())
            {
                ref var loading = ref iterator.Current;
                try
                {
                    if (LoadingUpdate(ref loading))
                    {
                        AssetLoadingDict.Remove(loading.Path);
                        AssetLoadings.RemoveAt(iterator.Index);
                    }
                }
                catch (Exception ex)
                {
                    var log = loading.Path;
                    AssetLoadingDict.Remove(loading.Path);
                    AssetLoadings.RemoveAt(iterator.Index);
                    throw new Exception(log, ex);
                }
            }
        }

        /// <summary>
        /// 加载中更新
        /// </summary>
        private static bool LoadingUpdate(ref AssetLoading loading)
        {
            if (loading.LoadDependenciesCount == 0)
            {
                if (AssetLoadeds.TryGetValue(loading.Path, out var loaded))
                {
                    CallLoadComplete(loaded, loading);
                    return true;
                }

                loading.LoadDependenciesCount = -1;
            }

            if (loading.LoadDependenciesCount < 0 && loading.Load())
            {
                if (loading.MainAsset == null)
                {
                    CallLoadComplete(null, loading);
                }
                else
                {
                    var loaded = new AssetLoaded
                    {
                        Bundle = loading.Bundle,
                        Path = loading.Path,
                        MainAsset = loading.MainAsset,
                    }.Initialize();
                    AssetLoadeds.GetValueOrAdd(loaded.Path) = loaded;
                    CallLoadComplete(loaded, loading);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        private static void CallLoadComplete(AssetLoaded loaded, in AssetLoading loading)
        {
            if (loaded != null)
            {
                var t = Time.time;
                _nextTryUnloadIdleCount = 0;
                if (_nextTryUnloadTime < t)
                    _nextTryUnloadTime = t + loaded.CacheTime + 2;
            }
            for (var i = 0; i < loading.CompletionDatas.Count; i++)
            {
                if (loading.CompletionDatas[i].Reference.IsValid)
                {
                    loaded?.References.Add(loading.CompletionDatas[i].Reference);
                }
                var call = loading.CompletionDatas[i].Call;
                try
                {
                    if (loaded != null)
                    {
                        call.ProgressCallback?.Invoke(loading, i);
                        var isValid = !loading.CompletionDatas[i].ReferenceIsValid || loading.CompletionDatas[i].Reference.IsValid;
                        if (call.FinishTaskCompletion != null)
                        {
                            if (isValid)
                                call.FinishTaskCompletion.TrySetResult(loaded);
                            else
                                call.FinishTaskCompletion.TrySetCanceled();
                        }
                        else if (isValid)
                        {
                            call.FinishCallback?.Invoke(new AssetLoading.Result
                            {
                                Loaded = loaded,
                                UserData = loading.CompletionDatas[i].UserData
                            });
                        }
                    }
                    else
                    {
                        if (call.FinishTaskCompletion != null)
                        {
                            call.FinishTaskCompletion.TrySetException(new Exception("not found asset: " + loading.Path));
                        }
                        else
                        {
                            Log.Error?.Write("not found asset: " + loading.Path);
                            call.FinishCallback?.Invoke(new AssetLoading.Result
                            {
                                Loaded = null,
                                UserData = loading.CompletionDatas[i].UserData
                            });
                        }
                    }
                }
                catch (Exception err)
                {
                    Log.Error?.Write("call complete error: " + call + "\n" + loading.Path + "\n" + err);
                }
            }
        }

        /// <summary>
        ///加载资源
        /// </summary>
        public static UniTask<AssetLoaded> Load(in AssetLoaderPath path, AssetLoading.ProgressDelegate progressHandle = null, bool isAlwaysNextFrame = false)
        {
            return Load(default, path, progressHandle, isAlwaysNextFrame);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static UniTask<AssetLoaded> Load(in AssetLoadReference reference, in AssetLoaderPath path, AssetLoading.ProgressDelegate progressHandle = null, bool isAlwaysNextFrame = false)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("path is empty");
            if (AssetLoadeds.TryGetValue(path, out var loaded) || ConstAssetLoadeds.TryGetValue(path, out loaded))
            {
                if (reference.IsValid)
                    loaded.References.Add(reference);
                return isAlwaysNextFrame ? UniTask.NextFrame().ContinueWith(() => loaded) : new UniTask<AssetLoaded>(loaded);
            }
            var completion = AutoResetUniTaskCompletionSource<AssetLoaded>.Create();
            LoadImpl(reference, path, new AssetLoading.CallbackData { FinishTaskCompletion = completion, ProgressCallback = progressHandle }, null);
            return completion.Task;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public static void Load(in AssetLoadReference reference, in AssetLoaderPath path, Action<AssetLoading.Result> finishCallback, object userData = null, AssetLoading.ProgressDelegate progressHandle = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("path is empty");
            if (AssetLoadeds.TryGetValue(path, out var loaded) || ConstAssetLoadeds.TryGetValue(path, out loaded))
            {
                if (reference.IsValid)
                    loaded.References.Add(reference);
                finishCallback?.Invoke(new AssetLoading.Result() { Loaded = loaded, UserData = userData });
            }
            else
            {
                LoadImpl(reference, path, new AssetLoading.CallbackData { FinishCallback = finishCallback, ProgressCallback = progressHandle }, userData);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private static ref AssetLoading LoadImpl(in AssetLoadReference reference, in AssetLoaderPath path, AssetLoading.CallbackData completeCallData, object userData)
        {
            // 首次加载
            if (!AssetLoadingDict.TryGetValue(path, out var loadingIndex))
            {
                ref var loading = ref AssetLoadings.AddEmpty(out loadingIndex);
                AssetLoadingDict.Add(path, loadingIndex);
                loading.Path = path;
                loading.CompletionDatas = new List<AssetLoading.CompletionData>();
#if ASSET_BUNDLE || !UNITY_EDITOR
                var dependencies = Info.AssetMetas?[path.Value].Dependencies;
                if (dependencies?.Length > 0)
                {
                    loading.LoadDependenciesCount = dependencies.Length;
                    if (AssetLoadings.Allocate(AssetLoadings.Count + dependencies.Length))
                        loading = ref AssetLoadings[loadingIndex];
                    foreach (var depPath in dependencies)
                    {
                        if (AssetLoadeds.TryGetValue(depPath, out var loaded) || ConstAssetLoadeds.TryGetValue(depPath, out loaded))
                        {
                            loaded.References.Add(path);
                            loading.LoadDependenciesCount--;
                        }
                        else
                        {
                            Load(path, depPath, OnLoadDependenceComplete, path.Value);
                        }
                    }
                }
#endif
            }

            var completeData = new AssetLoading.CompletionData
            {
                Reference = reference,
                ReferenceIsValid = reference.IsValid,
                UserData = userData,
                Call = completeCallData,
            };
            AssetLoadings[loadingIndex].CompletionDatas.Add(completeData);
            return ref AssetLoadings[loadingIndex];
        }

#if ASSET_BUNDLE || !UNITY_EDITOR
        /// <summary>
        /// 加载依赖完成
        /// </summary>
        private static void OnLoadDependenceComplete(AssetLoading.Result obj)
        {
            var parentPath = (string)obj.UserData;
            if (AssetLoadingDict.TryGetValue(parentPath, out var loadingIndex))
                --AssetLoadings[loadingIndex].LoadDependenciesCount;
        }
#endif

        /// <summary>
        ///
        /// </summary>
        public static void RemoveLoading(in AssetLoaderPath path, Action<AssetLoading.Result> completeCallback)
        {
            using var iterator = AssetLoadings.GetEnumerator();
            while (iterator.MoveNext())
            {
                ref var v = ref iterator.Current;
                if (!path.IsEmpty && v.Path != path) continue;
                for (var i = 0; i < v.CompletionDatas.Count; i++)
                {
                    if (v.CompletionDatas[i].Call.FinishCallback != completeCallback) continue;
                    if (v.CompletionDatas[i].UserData is AutoResetUniTaskCompletionSource<AssetLoaded> task)
                    {
                        task.TrySetCanceled();
                    }

                    v.CompletionDatas.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// 移除资源引用
        /// </summary>
        public static bool RemoveAssetReference(string path, AssetLoadReference reference)
        {
            if (AssetLoadeds.TryGetValue(path, out var loaded))
            {
                for (var i = loaded.References.Count - 1; i >= 0; i--)
                {
                    if (loaded.References[i] == reference)
                    {
                        loaded.References.RemoveAt(i);
                        TryUnload(loaded);
                        return true;
                    }
                }
            }
            else
            {
                if (AssetLoadingDict.TryGetValue(path, out var loadingIndex))
                {
                    ref var loading = ref AssetLoadings[loadingIndex];
                    for (var i = loading.CompletionDatas.Count - 1; i >= 0; i--)
                    {
                        if (loading.CompletionDatas[i].Reference == reference)
                        {
                            loaded.References.RemoveAt(i);
                            TryUnload(loaded);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试卸载全部已经加载的资源
        /// </summary>
        public static int TryUnloadAll(bool isForce = false)
        {
            var releaseCount = 0;
            using var iterator = AssetLoadeds.GetEnumerator();
            while (iterator.MoveNext())
            {
                if (TryUnload(iterator.Value, isForce))
                    ++releaseCount;
            }
            return releaseCount;
        }

        /// <summary>
        /// 尝试卸载资源
        /// </summary>
        public static bool TryUnload(AssetLoaded loaded, bool isForce = false)
        {
            if (isForce)
            {
                loaded.Unload();
                AssetLoadeds.Remove(loaded.Path);
                return true;
            }
            if (loaded.LoadedScene.IsValid() || Time.time - loaded.LastActiveTime < loaded.CacheTime) return false;
            var refCount = loaded.References.Count;
            if (refCount != 0 && loaded.References.RemoveAll(Match) != refCount) return false;
            loaded.Unload();
            AssetLoadeds.Remove(loaded.Path);
            return true;

            static bool Match(AssetLoadReference v) => !v.IsValid;
        }

        /// <summary>
        /// 移动到常量资源
        /// </summary>
        public static void MoveToConstAsset(AssetLoaded loaded)
        {
            AssetLoadeds.Remove(loaded.Path);
            ConstAssetLoadeds.Add(loaded.Path, loaded);
        }

        /// <summary>
        ///
        /// </summary>
        public static void ClearAll()
        {
            Info = default;
            CDN = null;
            ConstAssetLoadeds.Clear();
            AssetLoadeds.Clear();
            AssetLoadings.Clear();
            AssetLoadingDict.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ExistsAsset(in AssetLoaderPath path)
        {
            return
#if ASSET_BUNDLE || !UNITY_EDITOR
                Info.AssetMetas.ContainsKey(path.Value);
#else
#if UNITY_6000_0_OR_NEWER
                UnityEditor.AssetDatabase.AssetPathExists(path.FullPath);
#else
                File.Exists(path.FullPath);
#endif
#endif
        }
    }
}
