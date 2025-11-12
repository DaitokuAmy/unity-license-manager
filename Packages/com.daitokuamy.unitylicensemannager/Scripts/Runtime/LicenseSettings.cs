using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
#endif

namespace UnityLicenseManager {
    /// <summary>
    /// License管理用の設定ファイル
    /// </summary>
    public class LicenseSettings : ScriptableObject {
        /// <summary>
        /// ライセンス情報
        /// </summary>
        [Serializable]
        private class LicenseInfo {
            [Tooltip("アクティブ状態")]
            public bool isActive;
            [Tooltip("ライセンスアセット")]
            public TextAsset asset;
        }

        private static LicenseSettings s_instance;

        [SerializeField, Tooltip("ライセンス情報リスト")]
        private LicenseInfo[] _licenseInfos = Array.Empty<LicenseInfo>();

        /// <summary>シングルトンインスタンス</summary>
        public static LicenseSettings Instance {
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

        /// <summary>
        /// ライセンステキストの生成
        /// </summary>
        public string CreateLicensesText() {
            var text = new StringBuilder();
            var first = true;
            foreach (var info in _licenseInfos) {
                if (!info.isActive || info.asset == null) {
                    continue;
                }

                if (!first) {
                    text.AppendLine();
                }

                first = false;
                text.AppendLine(info.asset.text);
            }
            
            return text.ToString();
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