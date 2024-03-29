# スクリプトオプション

## オプションの種類

| オプション名      | 記号      | 概要 |
| ----------------- | --------- | ---- |
| None              |           | オプション指定しなかった場合 |
| NoRun             | n         | 実行しない |
| WaitForExit       | w         | 終了待ち(同期実行) |
| RunAsAdmin        | a         | 管理者として実行 |
| DomainPCOnly      | m         | ドメイン参加PCの場合のみ実行 |
| WorkgroupPCOnly   | k         | ワークグループPCの場合のみ実行 |
| SystemAccountOnly | s         | システムアカウントの場合のみ実行 |
| DomainUserOnly    | d         | ドメインユーザーの場合のみ実行 |
| LocalUserOnly     | l         | ローカルユーザーの場合のみ実行 |
| DGReachableOnly   | p         | デフォルトゲートウェイと通信可能な場合のみ実行 |
| TrustedOnly       | t         | 管理者に昇格している場合のみ実行 |
| Output            | o         | 実行中の標準/エラー出力をファイルに出力 |
| BeforeWait        | (数字)r   | 実行前に指定秒待機 |
| AfterWait         | r(数字)   | 実行後に指定秒待機 |

## オプションの詳細

### None

スクリプトファイルにオプション指定用の記号を記載していない状態。  
スクリプトファイルの主な挙動は以下のようになります。
- Language.jsonで登録済みの拡張子(言語)の場合、実行。
- 非同期で実行。
- 全ユーザー/全PCで実行。

### NoRun

スクリプトファイルに``[n]``オプションを指定した場合。
スクリプトファイルは実行されません。  
その他のオプションを指定してても無視されます。

### WaitForExit

スクリプトファイルに``[w]``オプションを指定した場合。
スクリプトの主な挙動は以下のようになります。
- 同期実行。※このスクリプトファイルが完了してから、次のスクリプトを実行する。

### RunAsAdmin

スクリプトファイルに``[a]``オプションを指定した場合。
スクリプトの主な挙動は以下のようになります。
- スクリプト実行時、``RunAs``オプションを付けて実行。(管理者として実行)
- ユーザーアカウント制御が「通知しない」に設定されている場合のみ実行。
- 管理者権限(Administratorsグループに所属)しているかどうかは判定しない。

### DomainPCOnly

スクリプトファイルに``[m]``オプションを指定した場合。
スクリプトの主な挙動は以下のようになります。
- ドメイン参加済みかどうかをチェックし、ドメイン参加済みであれば実行。

### WorkgroupPCOnly

スクリプトファイルに``[k]``オプションを指定した場合。
スクリプトの主な挙動は以下のようになります。
- ドメイン参加済みかどうかをチェックし、ドメイン参加していなければ実行。

### SystemAccountOnly

スクリプトファイルに``[s]``オプションを指定した場合。  
スクリプトの主な挙動は以下のようになります。
- システムアカウントで実行されている場合のみ実行。
- システムアカウントは、``Win32_SystemAccount``から確認して判定。

### DomainUserOnly

スクリプトファイルに``[d]``を指定した場合。  
スクリプトの主な挙動は以下のようになります。
- ワークグループPCの場合は実行しない。
- ドメインユーザーで実行されている場合のみ実行。

### LocalUserOnly

スクリプトファイルに``[l]``を指定した場合。  
スクリプトの主な挙動は以下のようになります。
- ローカルユーザーで実行されている場合のみ実行。
- システムアカウントでも実行。

### DGReachableOnly

スクリプトファイルに``[p]``を指定した場合。  
スクリプトの主な挙動は以下のようになります。
- 実行前にデフォルトゲートウェイ宛にPingを送信し、応答がある場合のみ実行。
- デフォルトゲートウェイが設定されていない場合は実行しない。
- デフォルトゲートウェイが複数設定されている場合、いずれか1つで応答があれば実行。
- IPv4、IPv6の両方でPing応答有無をチェック。

### TrustedOnly

スクリプトファイルに``[t]``を指定した場合。  
スクリプトの主な挙動は以下のようになります。
- 「管理者として実行」で実行されている。  
  ※最近のWindowsの場合、初期状態でAdministratorやSystemは「管理者として実行」される。
- 通常ユーザーがスクリプトオプション``[a]``を指定して通常実行しただけの場合は、実行しない。

### Output

スクリプトファイルに``[o]``を指定した場合。(アルファベットの「オー」)  
設定ファイルの``DefaultOutput``で``true``を指定した場合は、デフォルト状態で指定した状態となります。  
スクリプトの主な挙動は以下のようになります。
- スクリプト内で標準出力する処理がある場合、設定ファイルの``OutputPath``のフォルダー配下に出力する。

### BeforeWait

スクリプトファイルに``[数字r]``を指定した場合。  
例⇒ ``[3r]`` 等  
スクリプトの主な挙動は以下のようになります。
- 指定した秒数待機した後、スクリプトを実行。

### AfterWait

スクリプトファイルに``[r数字]]``を指定した場合。  
例⇒ ``[r3]`` 等  
BeforeWaitと同時に指定が可能な為、``[5r3]``のように指定することも可能。  
スクリプトオプションを指定する際、``[]``は複数記述できる為、``[r3][5r]``と指定することも可能。  
スクリプトの主な挙動は以下のようになります。
- スクリプト実行後、指定した秒数待機した後、次のスクリプトの処理に進む。

## サンプル

### スクリプトファイル名

各用途別のオプション指定方法を紹介します。

#### オプション指定無し

::: tip Filesフォルダー
01_Example.bat  
02_Example.bat  
03_Example.bat  
11_Example.bat  
12_Example.bat  
:::

設定ファイルで``Ranges``の設定を以下のように設定していた場合、

<code-group>
<code-block title="JSON" active>

```json
(前略)
  "Ranges": {
    "StartupScript": "0-9",
    "ShutdownScript": "11-29",
    "LogonScript": "81-89",
    "LogoffScript": "91-99"
  }
(後略)
```

</code-block>
<code-block title="Text">

```yml
(前略)
Ranges:
  StartupScript: 0-9
  ShutdownScript: 11-29
  LogonScript: 81-89
  LogoffScript: 91-99
```

</code-block>
</code-group>

``StartupScript.exe``で実行 ⇒ 01_Example.bat、02_Example.bat、03_Example.bat の順番で実行。非同期実行の為、完了順番は保障されない。  
``ShutdownScript.exe``で実行 ⇒ 11_Example.bat、12_Example.batの順番で実行。非同期実行の為、完了順番は保障されない。

実行フロー

![実行フロー01](./flow01.drawio.svg)





