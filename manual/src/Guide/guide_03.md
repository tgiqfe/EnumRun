# 制御情報

## 制御情報ファイルの種類

EnumRunの制御情報を記述するファイルは、以下の2種類があります。  
- 設定ファイル (Setting)
- 言語ファイル (Language)

### 設定ファイル

| ファイル名 | 拡張子 | 役割 |
| ---------- | ------ | ---- |
| Setting.json | .json | EnumRunの制御情報を記載。JSONファイル。 |
| Setting.yml | .yml or .yaml | EnumRunの制御情報を記載。Ymlファイル。 |
| Setting.txt | .txt | EnumRunの制御情報を記載。Textファイル。 |

``Setting.json``、``Setting.yml``、``Setting.txt``のうち、どれか一つのファイルは必須ファイルの為、予め準備しておく必要があります。

::: tip
本ドキュメントでは、``Setting.json``、``Setting.yml``、``Setting.txt``をまとめて「設定ファイル」と表記します。
:::

### 言語ファイル

| ファイル名 | 拡張子 | 役割 |
| ---------- | ------ | ---- |
| Language.json | .json | スクリプトの実行言語の制御情報を記載。 |
 
``Language.json``について、基本的にデフォルト設定のままで問題ありません。新しい言語の実行環境をインストールしたときに追記する為に使用します。  
また、``Language.json``は任意ファイルの為、予め準備しておく必要はありません。

## 作成

### 新規ファイル作成

エディタソフトを使用して空ファイルを作成し、以下の文字コード/改行コードで保存します。

| ファイル名    | 文字コード | 改行コード |
| ------------- | ---------- | ---------- |
| Setting.json  | UTF8 with BOM | CRLF |
| Setting.yml   | UTF8 with BOM | CRLF |
| Setting.txt   | UTF8 without BOM / UTF8 with BOM / Shift-JIS | CRLF |
| Language.json | UTF8 with BOM | CRLF |

※比較的最近のWindows 10のメモ帳でファイルを作成した場合、``UTF8 without BOM``で保存されます。  
※``Setting.txt``は上表の3つの文字コードでのみ動作確認済みです。他の文字コード(EUC-JP等)も利用できる可能性はありますが、非推奨です。

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
    "LogonScript": "11-29",
    "LogoffScript": "81-89",
    "ShutdownScript": "91-99"
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
  LogonScript: 11-29
  LogoffScript: 81-89
  ShutdownScript: 91-99
```

Yamlの記述ルールに近いのですが、オリジナルの記述ルールです。  
主な設定は、  
★ここに設定ファイルの記述解説ページへのリンク  
★ここにサンプルページへのリンク  
を参照。


