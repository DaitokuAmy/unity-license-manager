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
            serializedObject.Update();

            var scrollHeight = Mathf.Min(300, _licenseInfoList.GetHeight());
            using (var scrollScope = new EditorGUILayout.ScrollViewScope(_listScroll, GUILayout.Height(scrollHeight))) {
                // Display List
                _licenseInfoList.DoLayoutList();

                _listScroll = scrollScope.scrollPosition;
            }

            EditorGUILayout.Space();

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
                        _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue = string.Empty;
                        _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("asset").objectReferenceValue = asset;
                        _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("text").stringValue = string.Empty;
                        _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("isActive").boolValue = true;
                    }
                }

                if (GUILayout.Button("Sort")) {
                    for (var start = 0; start < _licenseInfosProp.arraySize - 2; start++) {
                        var currentIndex = start;
                        var currentName = _licenseInfosProp.GetArrayElementAtIndex(currentIndex).FindPropertyRelative("name").stringValue;
                        for (var i = start + 1; i < _licenseInfosProp.arraySize; i++) {
                            var elementProp = _licenseInfosProp.GetArrayElementAtIndex(i);
                            var targetName = elementProp.FindPropertyRelative("name").stringValue;
                            if (string.Compare(currentName, targetName, StringComparison.Ordinal) > 0) {
                                currentIndex = i;
                                currentName = targetName;
                            }
                        }

                        if (currentIndex != start) {
                            _licenseInfosProp.MoveArrayElement(currentIndex, start);
                        }
                    }
                }
            }

            // Preview
            if (_licenseInfoList.selectedIndices.Count > 0) {
                var index = _licenseInfoList.selectedIndices[0];
                var nameProp = _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("name");
                var assetProp = _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("asset");
                var textProp = _licenseInfosProp.GetArrayElementAtIndex(index).FindPropertyRelative("text");
                var textAsset = assetProp.objectReferenceValue as TextAsset;
                using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Preview", EditorStyles.objectFieldThumb);
                    EditorGUILayout.PropertyField(nameProp);
                    using (var scope = new EditorGUI.ChangeCheckScope()) {
                        EditorGUILayout.PropertyField(assetProp, new GUIContent("License"));
                        if (scope.changed) {
                            if (assetProp.objectReferenceValue != null) {
                                textProp.stringValue = string.Empty;
                            }
                        }
                    }

                    var text = textAsset != null ? textAsset.text : textProp.stringValue;
                    var height = EditorStyles.textArea.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth);
                    var useTextAsset = textAsset != null;
                    using (new EditorGUI.DisabledScope(useTextAsset)) {
                        using (var scope = new EditorGUI.ChangeCheckScope()) {
                            text = EditorGUILayout.TextArea(text, EditorStyles.textArea, GUILayout.Height(height));
                            if (scope.changed) {
                                textProp.stringValue = text;
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope()) {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Copy Clipboard")) {
                            GUIUtility.systemCopyBuffer = text;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// ライセンスのヘッダー表示用テキストを取得
        /// </summary>
        private string GetLicenseHeaderText(SerializedProperty elementProp) {
            if (elementProp == null) {
                return "Unknown";
            }

            var nameProp = elementProp.FindPropertyRelative("name");
            if (!string.IsNullOrEmpty(nameProp.stringValue)) {
                return nameProp.stringValue;
            }

            var assetProp = elementProp.FindPropertyRelative("asset");
            var textProp = elementProp.FindPropertyRelative("text");
            var textAsset = assetProp.objectReferenceValue as TextAsset;
            var text = textAsset != null ? textAsset.text : textProp.stringValue;

            var lines = text.Split("\n").ToArray();
            if (lines.Length <= 0) {
                return "Unknown";
            }

            var result = lines.FirstOrDefault(x => x.ToLower().Contains("copyright"));
            if (string.IsNullOrEmpty(result)) {
                result = lines[0];
            }

            return $"<{result}>";
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
                var textProp = elementProp.FindPropertyRelative("text");

                // TextAssetがMissingしていたら自動削除
                if (assetProp.objectReferenceValue == null &&
                    assetProp.objectReferenceInstanceIDValue != 0 &&
                    string.IsNullOrEmpty(textProp.stringValue)) {
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
            _licenseInfoList = new ReorderableList(serializedObject, _licenseInfosProp, true, true, true, true);
            _licenseInfoList.multiSelect = true;
            _licenseInfoList.drawHeaderCallback += rect => {
                EditorGUI.LabelField(rect, "Licenses");
            };
            _licenseInfoList.drawElementCallback += (rect, index, active, focused) => {
                var elementProp = _licenseInfosProp.GetArrayElementAtIndex(index);
                var label = new GUIContent(GetLicenseHeaderText(elementProp));
                var isActiveProp = elementProp.FindPropertyRelative("isActive");

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