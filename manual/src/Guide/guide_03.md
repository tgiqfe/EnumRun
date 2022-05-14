# 制御情報

## 制御情報ファイルの種類

設定ファイルは、以下の3種類があります。  

| ファイル名 | 拡張子 | 役割 |
| ---------- | ------ | ---- |
| Setting.json | .json | EnumRunの制御情報を記載。JSONファイル。 |
| Setting.txt | .txt | EnumRunの制御情報を記載。Textファイル。<br>Setting.jsonとSetting.txtが同じフォルダー内にある場合は、Setting.jsonを優先します。 |
| Language.json | .json | スクリプトの実行言語の制御情報を記載。 |

``Setting.json``あるいは``Setting.txt``のどちらかのファイルは必須です。  
``Language.json``について、基本的にデフォルト設定のままで問題ありません。新しい言語の実行環境をインストールしたときに追記する為に使用します。

::: tip
本ドキュメントでは、``Setting.json``もしくは``Setting.txt``をまとめて「設定ファイル」と表記します。
:::

## 作成

### 新規ファイル作成

エディタソフトを使用して空ファイルを作成し、以下の文字コード/改行コードで保存します。

| ファイル名 | 文字コード | 改行コード |
| ---------- | ---------- | ---------- |
| Setting.json | UTF8 with BOM | CRLF |
| Setting.txt | UTF8 without BOM / UTF8 with BOM / Shift-JIS | CRLF |
| Language.json | UTF8 with BOM | CRLF |

※比較的最近のWindows 10のメモ帳でファイルを作成した場合、``UTF8 without BOM``で保存されます。  
※``Setting.txt``は上表の3つの文字コードでのみ動作確認済みですが、その他の文字コードも基本的に使用可能です。

### 設定ファイルを記述(Setting.json)

JSONファイルの記述ルール等の解説は割愛します。

以下のように記述してください。
```json
{
  "RestTime": 60,
  "RetentionPeriod": 10,
  "MinLogLevel": "Info",
  "Ranges": {
    "StartupScript": "0-9",
    "ShutdownScript": "11-29",
    "LogonScript": "81-89",
    "LogoffScript": "91-99"
  }
}
```

主な設定は、  
★ここに設定ファイルの記述解説ページへのリンク  
★ここにサンプルページへのリンク  
を参照。

### 設定ファイルを記述(Setting.txt)

以下のように記述してください。
```yml
RestTime: 60
RetentionPeriod: 10
MinLogLevel: Info
Ranges:
  StartupScript: 0-9
  ShutdownScript: 11-29
  LogonScript: 81-89
  LogoffScript: 91-99
```

Yamlの記述ルールに近いのですが、オリジナルの記述ルールです。  
主な設定は、  
★ここに設定ファイルの記述解説ページへのリンク  
★ここにサンプルページへのリンク  
を参照。


