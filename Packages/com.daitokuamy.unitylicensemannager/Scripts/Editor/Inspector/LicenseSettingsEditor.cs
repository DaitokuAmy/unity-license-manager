using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityLicenseManager.Editor {
    /// <summary>
    /// LicenseSettingsのエディタ拡張
    /// </summary>
    [CustomEditor(typeof(LicenseSettings))]
    public class LicenseSettingsEditor : UnityEditor.Editor {
        private SerializedProperty _licenseInfosProp;
        private ReorderableList _licenseInfoList;
        private Vector2 _listScroll;

        /// <inheritdoc/>
        public override void OnInspectorGUI() {
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_listScroll, GUILayout.ExpandHeight(true))) {
                serializedObject.Update();

                // Display List
                _licenseInfoList.DoLayoutList();

                using (new EditorGUILayout.HorizontalScope()) {
                    // Clear
                    if (GUILayout.Button("Clear")) {
                        _licenseInfosProp.ClearArray();
                        _licenseInfoList.ClearSelection();
                    }

                    // Auto Search
                    if (GUILayout.Button("Auto Search")) {
                        var currentAssets = new List<TextAsset>(_licenseInfosProp.arraySize);
                        for (var i = 0; i < _licenseInfosProp.arraySize; i++) {
                            var elementProp = _licenseInfosProp.GetArrayElementAtIndex(i);
                            var assetProp = elementProp.FindPropertyRelative("asset");
                            if (assetProp.objectReferenceValue is TextAsset asset) {
                                currentAssets.Add(asset);
                            }
                        }

                        var guids = AssetDatabase.FindAssets("t:textasset LICENSE", new[] { "Assets", "Packages" });
                        foreach (var guid in guids) {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var fileName = Path.GetFileNameWithoutExtension(path).ToLower();
                            if (fileName != "license") {
                                continue;
                            }

                            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                            if (currentAssets.Contains(asset)) {
                                continue;
                            }

                            var index = _licenseInfosProp.arraySize;
                            _licenseInfosProp.InsertArrayElementAtIndex(index);
                            _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("asset").objectReferenceValue = asset;
                            _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("isActive").boolValue = true;
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();

                _listScroll = scrollScope.scrollPosition;
            }

            // Preview
            if (_licenseInfoList.selectedIndices.Count > 0) {
                var index = _licenseInfoList.selectedIndices[0];
                var assetProp = _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("asset");
                if (assetProp.objectReferenceValue is TextAsset textAsset) {
                    using (new EditorGUILayout.VerticalScope()) {
                        EditorGUILayout.LabelField("Preview", EditorStyles.objectFieldThumb);

                        var height = EditorStyles.label.CalcHeight(new GUIContent(textAsset.text), Screen.width);
                        EditorGUILayout.LabelField(textAsset.text, EditorStyles.objectField, GUILayout.Height(height));

                        using (new EditorGUILayout.HorizontalScope()) {
                            using (new EditorGUI.DisabledScope(true)) {
                                EditorGUILayout.ObjectField(textAsset, typeof(TextAsset), true);
                            }

                            if (GUILayout.Button("Copy Clipboard")) {
                                GUIUtility.systemCopyBuffer = textAsset.text;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ライセンスのヘッダー表示用テキストを取得
        /// </summary>
        private string GetLicenseHeaderText(TextAsset asset) {
            if (asset == null) {
                return "Unknown";
            }

            var lines = asset.text.Split("\n").ToArray();
            if (lines.Length <= 0) {
                return "Unknown";
            }

            var result = lines.FirstOrDefault(x => x.ToLower().Contains("copyright"));
            if (string.IsNullOrEmpty(result)) {
                result = lines[0];
            }

            return result;
        }

        /// <summary>
        /// リストの中身で不正なデータをリフレッシュ
        /// </summary>
        private void RefreshList(SerializedProperty prop) {
            serializedObject.Update();
            var dirty = false;
            for (var i = prop.arraySize - 1; i >= 0; i--) {
                var elementProp = prop.GetArrayElementAtIndex(i);
                var assetProp = elementProp.FindPropertyRelative("asset");
                if (assetProp.objectReferenceValue == null) {
                    prop.DeleteArrayElementAtIndex(i);
                    dirty = true;
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (dirty) {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        private void OnEnable() {
            // プロパティ取得
            _licenseInfosProp = serializedObject.FindProperty("_licenseInfos");

            // リストの中身をリフレッシュ
            RefreshList(_licenseInfosProp);

            // 描画用リスト構築
            _licenseInfoList = new ReorderableList(serializedObject, _licenseInfosProp, true, true, false, false);
            _licenseInfoList.drawHeaderCallback += rect => {
                EditorGUI.LabelField(rect, "Licenses");
            };
            _licenseInfoList.drawElementCallback += (rect, index, active, focused) => {
                var elementProp = _licenseInfosProp.GetArrayElementAtIndex(index);
                var label = new GUIContent(GetLicenseHeaderText(null));
                var assetProp = elementProp.FindPropertyRelative("asset");
                var isActiveProp = elementProp.FindPropertyRelative("isActive");
                if (assetProp.objectReferenceValue is TextAsset textAsset) {
                    label.text = GetLicenseHeaderText(textAsset);
                }

                var drawRect = rect;
                var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                drawRect.height = height;
                drawRect.width = 20;
                isActiveProp.boolValue = EditorGUI.ToggleLeft(drawRect, GUIContent.none, isActiveProp.boolValue);
                drawRect.width = rect.width;
                drawRect.xMin += 20;
                EditorGUI.LabelField(drawRect, label, EditorStyles.boldLabel);
                drawRect.xMin -= 20;
                drawRect.y += height;
            };
            _licenseInfoList.elementHeightCallback += index => {
                var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                return height;
            };

            // 選択状態初期化
            if (_licenseInfoList.count > 0) {
                _licenseInfoList.Select(0);
            }
        }

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        private void OnDisable() {
            _licenseInfoList = null;
            _licenseInfosProp = null;
        }
    }
}