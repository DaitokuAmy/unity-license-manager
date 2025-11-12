# unity-license-manager
Unity内で利用されているライセンス情報を管理するための機能  
<img width="434" height="686" alt="image" src="https://github.com/user-attachments/assets/d276f573-0235-4fdd-864a-848b79534851" />

## 概要
#### 特徴
* Projectに含まれているLICENSE.mdファイルを自動的に検索し、一覧化してくれる
* LICENSE.mdがない物も入力する形で追記可能
* ライセンスとは別に表示用の名称（システム名など）を設定する事が出来る

#### 背景
Unityプロジェクトでアプリリリース時にライセンス表記を行う事があるが、その管理をアセットとして一元化したかった

## セットアップ
#### インストール
1. Window > Package ManagerからPackage Managerを開く
2. 「+」ボタン > Add package from git URL
3. 以下を入力してインストール
   * https://github.com/DaitokuAmy/unity-license-manager.git?path=/Packages/com.daitokuamy.unitylicensemanager
   ![image](https://user-images.githubusercontent.com/6957962/209446846-c9b35922-d8cb-4ba3-961b-52a81515c808.png)

あるいはPackages/manifest.jsonを開き、dependenciesブロックに以下を追記します。

```json
{
    "dependencies": {
        "com.daitokuamy.unitylicensemanager": "https://github.com/DaitokuAmy/unity-license-manager.git?path=/Packages/com.daitokuamy.unitylicensemanager"
    }
}
```
バージョンを指定したい場合には以下のように記述します。

https://github.com/DaitokuAmy/unity-license-manager.git?path=/Packages/com.daitokuamy.unitylicensemanager#1.0.0
