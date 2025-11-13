namespace UnityLicenseManager {
    /// <summary>
    /// ライセンス情報アクセス用インターフェース
    /// </summary>
    public interface ILicenseInfo {
        /// <summary>システム名</summary>
        string Name { get; }
        /// <summary>ライセンス表記</summary>
        string License { get; }
    }
}