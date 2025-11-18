using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace UnityLicenseManager {
    /// <summary>
    /// License管理用の設定ファイル
    /// </summary>
    public class LicenseSettings : ScriptableObject {
        /// <summary>
        /// フィルター情報
        /// </summary>
        [Serializable]
        public class FilterInfo {
            public string[] folderPaths;
            public string[] fileNames;
        }
        
        /// <summary>
        /// ライセンス情報
        /// </summary>
        [Serializable]
        private class LicenseInfo {
            [Tooltip("アクティブ状態")]
            public bool isActive;
            [Tooltip("システム名")]
            public string name;
            [Tooltip("ライセンスアセット")]
            public TextAsset asset;
            [Tooltip("テキスト")]
            public string text;
        }

        /// <summary>
        /// 返却用のライセンス情報
        /// </summary>
        private class InternalLicenseInfo : ILicenseInfo {
            public string Name { get; }
            public string License { get; }

            public InternalLicenseInfo(string name, string license) {
                Name = name;
                License = license;
            }
        }

        private static LicenseSettings s_instance;

        [SerializeField, Tooltip("ライセンス情報リスト")]
        private LicenseInfo[] _licenseInfos = Array.Empty<LicenseInfo>();
        [SerializeField, Tooltip("オートサーチ時の検索フィルター情報リスト")]
        private FilterInfo[] _searchFilterInfos = Array.Empty<FilterInfo>();

        /// <summary>シングルトンインスタンス</summary>
        private static LicenseSettings Instance {
            get {
                if (s_instance != null) {
                    return s_instance;
                }

#if UNITY_EDITOR
                var settings = Resources.FindObjectsOfTypeAll<LicenseSettings>().FirstOrDefault();
                if (settings != null) {
                    var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
                    preloadedAssets.Add(settings);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                    AssetDatabase.SaveAssets();
                }

                s_instance = settings;
#endif

                return s_instance;
            }
        }
        
        /// <summary>オートサーチ時の検索フィルター情報リスト</summary>
        public FilterInfo[] SearchFilterInfos => _searchFilterInfos;

        /// <summary>
        /// ライセンス情報の取得
        /// </summary>
        public static ILicenseInfo[] GetLicenseInfos() {
            if (Instance == null) {
                return Array.Empty<ILicenseInfo>();
            }

            return Instance.GetLicenseInfosInternal();
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            if (s_instance == null) {
                s_instance = this;
            }
        }

        /// <summary>
        /// ライセンス情報の取得
        /// </summary>
        private ILicenseInfo[] GetLicenseInfosInternal() {
            var result = new List<ILicenseInfo>();
            foreach (var info in _licenseInfos) {
                if (!info.isActive) {
                    continue;
                }

                if (string.IsNullOrEmpty(info.name)) {
                    continue;
                }

                var license = info.asset != null ? info.asset.text : info.text;
                if (string.IsNullOrEmpty(license)) {
                    continue;
                }

                result.Add(new InternalLicenseInfo(info.name, license));
            }

            return result.ToArray();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 設定ファイルの生成処理
        /// </summary>
        [MenuItem("Assets/Create/Unity License Manager/Settings")]
        private static void CreateSettings() {
            // 既に存在していたらエラー
            var config = PlayerSettings.GetPreloadedAssets().OfType<LicenseSettings>().FirstOrDefault();
            if (config != null) {
                throw new InvalidOperationException($"{nameof(LicenseSettings)} already exists in preloaded assets.");
            }

            var assetPath = EditorUtility.SaveFilePanelInProject($"Save {nameof(LicenseSettings)}", nameof(LicenseSettings), "asset", "", "Assets");
            if (string.IsNullOrEmpty(assetPath)) {
                return;
            }

            // フォルダがなかったら作る
            var folderPath = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(folderPath) && !Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            // アセットを作成してPreloadedAssetsに設定
            var instance = CreateInstance<LicenseSettings>();
            AssetDatabase.CreateAsset(instance, assetPath);
            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.Add(instance);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            AssetDatabase.SaveAssets();
        }
#endif
    }
}