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

## 使用方法
#### 設定ファイルの作成
以下のように設定ファイルを任意の場所に作成します  
<img width="703" height="667" alt="image" src="https://github.com/user-attachments/assets/3ab1ac31-ae55-4aa8-bce6-2f7916e6f0e0" />

#### ライセンス情報の設定
**Auto Search** ボタンを押す事で、プロジェクトに含まれる LICENSE.md を検索し、ライセンス表記を自動抽出する  
<img width="798" height="197" alt="image" src="https://github.com/user-attachments/assets/9c9a3e66-7ad2-4214-a3fa-8064c53ceff8" />
<img width="479" height="645" alt="image" src="https://github.com/user-attachments/assets/b613a241-84d3-496b-a970-ed5b4751c3aa" />

Licenseファイルを使わない場合、**License** に設定された TextAsset を Noneにする事で直接テキストを入力する事が可能
<img width="476" height="304" alt="image" src="https://github.com/user-attachments/assets/6157dae1-2fc7-49df-8499-953e7fdcb395" />

**Name** にライセンス表記時にヘッダーとして出すシステム名称などを入力する  
※空文字の場合は取得時にスキップされます

#### プログラムから取得する
```
// static関数を使ってライセンス情報を配列として取得できる
var licenseInfos = LicenseSettings.GetLicenseInfos();
var licenseText = new StringBuilder();
var first = true;
foreach (var info in licenseInfos) {
    if (!first) {
        licenseText.AppendLine();
    }
    first = false;

    // Name, License にそれぞれテキストが含まれているため、表記したいルールに合わせて利用する
    licenseText.AppendLine($"<b>{info.Name}</b>");
    licenseText.AppendLine(info.License);
}

_text.text = licenseText.ToString();
```
