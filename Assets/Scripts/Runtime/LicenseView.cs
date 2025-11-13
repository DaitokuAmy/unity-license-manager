using System.Text;
using TMPro;
using UnityEngine;
using UnityLicenseManager;

/// <summary>
/// ライセンス表示用のView
/// </summary>
public class LicenseView : MonoBehaviour {
    [SerializeField, Tooltip("表示テキスト")]
    private TMP_Text _text;

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start() {
        var licenseInfos = LicenseSettings.GetLicenseInfos();
        var licenseText = new StringBuilder();
        var first = true;
        foreach (var info in licenseInfos) {
            if (!first) {
                licenseText.AppendLine();
            }

            first = false;
            licenseText.AppendLine($"<b>{info.Name}</b>");
            licenseText.AppendLine(info.License);
        }
        
        _text.text = licenseText.ToString();
    }
}