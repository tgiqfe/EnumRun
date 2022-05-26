# ログ転送(ScriptDelivery)

## クライアント側設定

設定ファイルに``ScriptDelivery``パラメータ配下の``LogTransport``を``true``に設定することで、ScriptDeliveryサーバへログを転送することができます。  
このとき、``Server``の値に、ログ転送先のScriptDeliveryサーバのURLも設定しておく必要があります。

例)

<code-group>
<code-block title="JSON" active>

```json{16-18,21}
{
  "FilesPath": "Files",
  "LogsPath": "Logs",
  "OutputPath": "Output",
  "RestTime": 60,
  "DefaultOutput": false,
  "RetentionPeriod": 7,
  "MinLogLevel": "info",
  "Ranges": {
    "StartupScript": "0-9",
    "ShutdownScript": "11-29",
    "LogonScript": "81-89",
    "LogoffScript": "91-99"
  },
  "ScriptDelivery": {
    "Server": [
      "http://localhost:5000"
    ],
    "Process": "EnumRun",
    "TrashPath": "C:\\App\\EnumRun\\TrashPath",
    "LogTransport": true
  }
}
```

</code-block>
<code-block title="Text">

```yml{14,17}
FilesPath: Files
LogsPath: Logs
OutputPath: Output
RestTime: 60
DefaultOutput: False
RetentionPeriod: 7
MinLogLevel: info
Ranges:
  StartupScript: 0-9
  ShutdownScript: 11-29
  LogonScript: 81-89
  LogoffScript: 91-99
ScriptDelivery:
  Server: http://localhost:5000
  Process: EnumRun
  TrashPath: C:\App\EnumRun\TrashPath
  LogTransport: true
```

</code-block>
</code-group>

### サーバURL指定

ScriptDeliveryサーバは、複数指定することができます。  
(サーバ側冗長構成の場合に利用)

JSONの場合はstring配列の記述ルールに従って記述してください。  
Textの場合は、``,``区切りで複数サーバを記述してください。

<code-group>
<code-block title="JSON" active>

```json
(前略)
  "ScriptDelivery": {
    "Server": [
      "http://localhost:5000",
      "http://192.168.1.101:5001",
      "https://192.168.2.102:5001"
    ],
    "Process": "EnumRun",
    "TrashPath": "C:\\App\\EnumRun\\TrashPath",
    "LogTransport": true
  }
```

</code-block>
<code-block title="Text">

```yml
(前略)
ScriptDelivery:
  Server: http://localhost:5000, http://192.168.1.101:5001, https://192.168.2.102:5001
  Process: EnumRun
  TrashPath: C:\App\EnumRun\TrashPath
  LogTransport: true
```

</code-block>
</code-group>

### ログ転送設定フラグ

``LogTransport``へ設定するフラグは、以下のように設定してください。

| 形式 | true | false | 備考 |
| ---- | ---- | ----- | ---- |
| JSON | true | false | 全て小文字で、true / false を記述。 |
| Text | falseに一致する文字以外の全て | false、(空欄)、"null"、"0" | falseとして判定する文字のみが、内部的に予め設定しています。<br>falseとして判定する文字以外が記述されている場合に、trueとして判定します。 |

``true``に設定する場合の例
```json
"LogTransport": true
```
```
LogTransport: true
LogTransport: 1
LogTransport: A
```

``false``に設定する場合の例
```json
"LogTransport": false
```
```
LogTransport: false
LogTransport: 0
LogTransport:
```

<br>

::: tip
クライアント側の設定は基本的にこれだけです。  
:::



