//==================={By Qcbf|qcbf@qq.com|9/1/2023 11:07:20 AM}===================

// #define ASSET_BUNDLE

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace FLib.Unity
{
    public class AssetSyncer
    {
        public readonly struct ProgressInfo
        {
            public readonly float Value;
            public readonly string Label;

            public ProgressInfo(float value, string label)
            {
                Label = label;
                Value = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public async UniTask StartAll(IProgress<ProgressInfo> progress)
        {
            progress?.Report(new ProgressInfo(0, "start"));
#if ASSET_BUNDLE || !UNITY_EDITOR
            await LoadLocalInfo();
            await DownloadCdnAssets(progress);
#else
            await UniTask.Yield();
#endif
        }

#if ASSET_BUNDLE || !UNITY_EDITOR
        /// <summary>
        ///
        /// </summary>
        public async UniTask LoadLocalInfo()
        {
            InputBlocker.Open(nameof(LoadLocalInfo));

            var infoPath = Path.Combine(Application.streamingAssetsPath, AssetLoader.GAME_RES_NAME, AssetLoader.INFO_FILE_NAME);
            try
            {
                using var req = UnityWebRequest.Get(infoPath);
                req.timeout = 1;
                await req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    BytesPack.Unpack(ref AssetLoader.Info, Compressor.Uncompress(req.downloadHandler.data));
                    Log.Info?.Write($"local internal: {AssetLoader.Info}");
                }
            }
            catch (Exception e)
            {
                Log.Info?.Write(e);
            }

            infoPath = Path.Combine(AssetLoader.PersistentAssetPath, AssetLoader.INFO_FILE_NAME);
            if (File.Exists(infoPath))
            {
                var externalInfo = AssetLoaderInfo.Unpack(File.ReadAllBytes(infoPath));
                Log.Info?.Write($"external info {externalInfo.ToString()}");
                if (externalInfo.Id > AssetLoader.Info.Id)
                {
                    AssetLoader.Info = externalInfo;
                    Log.Info?.Write("use external info");
                }
                else
                {
                    FIO.ClearDirectory(AssetLoader.PersistentAssetPath);
                    Log.Info?.Write("use internal info");
                }
            }

            if (AssetLoader.Info.IsEmpty)
                throw new Exception("not found info asset");

            InputBlocker.Close(nameof(LoadLocalInfo));
        }
#endif

#if ASSET_BUNDLE || !UNITY_EDITOR
        /// <summary>
        /// check and download new cdn asset files
        /// </summary>
        public async UniTask DownloadCdnAssets(IProgress<ProgressInfo> progress)
        {
            if (string.IsNullOrEmpty(AssetLoader.CDN))
                return;
            // download cdn info
            Log.Info?.Write($"download cdn asset {AssetLoader.CDN}");
            progress?.Report(new ProgressInfo(0.2f, "check asset updates"));
            using (var req = UnityWebRequest.Get(AssetLoader.CDN + AssetLoader.INFO_ID_FILE_NAME))
            {
                await req.SendWebRequest();
                if (req.downloadedBytes == 0 || Convert.ToInt32(req.downloadHandler.data) <= AssetLoader.Info.Id)
                    return;
            }
            var localInfo = AssetLoader.Info;
            byte[] infoBytes;
            progress?.Report(new ProgressInfo(0.3f, "download asset updates"));
            using (var req = UnityWebRequest.Get(AssetLoader.CDN + AssetLoader.INFO_FILE_NAME))
            {
                await req.SendWebRequest();
                infoBytes = req.downloadHandler.data;
                BytesPack.Unpack(ref AssetLoader.Info, Compressor.Uncompress(infoBytes));
            }
            Log.Info?.Write("use cdn info");
            using var downloadCDNFileMetas = new PooledList<AssetLoaderInfo.Meta>(128);
            using var writeAssets = new PooledList<Task>(64);

            // compare local files with cdn files, record new cdn files
            foreach (var (assetName, cdnFileMeta) in AssetLoader.Info.AssetMetas)
            {
                if (cdnFileMeta.Size == 0)
                    continue;
                if (localInfo.AssetMetas?.TryGetValue(assetName, out var localMeta) != true)
                {
                    downloadCDNFileMetas.Add(cdnFileMeta);
                }
                else if (localMeta != cdnFileMeta || !VerifyPersistentAsset(cdnFileMeta))
                {
                    downloadCDNFileMetas.Add(cdnFileMeta);
                    // clear obsolete file
                    File.Delete(Path.Combine(AssetLoader.PersistentAssetPath, localMeta.FileNameStr));
                }
            }

            // download cdn files
            var retryCount = 0;
            for (var i = downloadCDNFileMetas.Count - 1; i >= 0; i--)
            {
                var downloadCdnFileMeta = downloadCDNFileMetas[i];
                try
                {
                    var progressValue = Mathf.Lerp(0.5f, 0.9f, 1 - i / (float)downloadCDNFileMetas.Count);
                    progress?.Report(new ProgressInfo(progressValue, downloadCdnFileMeta.FileNameStr));
                    Log.Info?.Write($"download: {downloadCdnFileMeta.FileNameStr} {progressValue:p2}");
                    using var fileReq = UnityWebRequest.Get(AssetLoader.CDN + downloadCdnFileMeta.FileNameStr);
                    await fileReq.SendWebRequest();
                    if ((int)fileReq.downloadedBytes != downloadCdnFileMeta.Size)
                    {
                        if (++retryCount > 3)
                            throw new Exception($"download asset size error {fileReq.url}");
                        Log.Info?.Write($"size error redownload[{retryCount}]: {fileReq.downloadedBytes}/{downloadCdnFileMeta.Size} {downloadCdnFileMeta.FileNameStr}");
                        i--;
                        continue;
                    }
                    retryCount = 0;
                    _ = writeAssets.Add(File.WriteAllBytesAsync(Path.Combine(AssetLoader.PersistentAssetPath, downloadCdnFileMeta.FileNameStr), fileReq.downloadHandler.data));
                }
                catch (Exception ex)
                {
                    throw new Exception($"download error {downloadCdnFileMeta.FileNameStr}", ex);
                }
            }

            progress?.Report(new ProgressInfo(1, "the last"));
            await Task.WhenAll(writeAssets.Array);
            await File.WriteAllBytesAsync(Path.Combine(AssetLoader.PersistentAssetPath, AssetLoader.INFO_FILE_NAME), infoBytes);
        }

        public static bool VerifyPersistentAsset(in AssetLoaderInfo.Meta fileMeta)
        {
            var fileInfo = new FileInfo(Path.Combine(AssetLoader.PersistentAssetPath, fileMeta.FileNameStr));
            // 如果文件不存在假定在包内有，否则对比外部储存的文件大小是否一致
            return !fileInfo.Exists || fileInfo.Length == fileMeta.Size;
        }
#endif
    }
}
