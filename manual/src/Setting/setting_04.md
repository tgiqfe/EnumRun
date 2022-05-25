# ログ

## ログの種類

### 出力先種別

EnumRunのログは、以下の場所に出力されます。

| 出力方式 | 概要 |
| -------- | ---- |
| ファイル | ローカルPC内にファイルとして出力。 |
| Logstash | Elastic StackのLogstashに対してログ転送。<br>Elasticsearchへの直接送信や、Filebeatには未対応。 |
| Syslog   | Syslogサーバへログ転送 |
| ScriptDelivery | ScriptDevliveryサーバへログ転送 |

### ログ種類

EnumRunのログは、以下のログが出力されます。

| ログ名     | 出力タイミング | 概要 |
| ---------- | -------------- | ---- |
| ProcessLog | アプリケーション実行中 | 各処理の情報や実行結果、エラー発生等を格納 |
| MachineLog | 1日に1回 | 実行中マシンの情報を格納 |
| SessionLog | アプリケーション実行時 | ユーザー/アプリケーションの情報を格納 |

## ログレベル

ログレベルは以下の5段階です。

| Level     | 値   | 概要 |
| --------- | ---- | ---- |
| Debug     | -1   | デバッグに使用する為の詳細情報 |
| Info      |  0   | 通常ログ情報 |
| Attention |  1   | 所定の作業は完了していないが、正常な判定結果に基づく通知情報 |
| Warn      |  2   | 軽度の問題の情報 |
| Error     |  3   | 重度の問題の情報 |

SyslogのSeverityとの対応表
| Level     | ログレベル値 | Severity      | Severity値 |
| --------- | ------------ | ------------- | ---------- |
| Debug     | -1           | debugging     | 7          |
| Info      |  0           | informational | 6          |
| Attention |  1           | notifications | 5          |
| Warn      |  2           | warnings      | 4          |
| Error     |  3           | errors, critical, alerts, emergencies | 3, 2, 1, 0 |







設定ファイル内の``LogsPath``パラメータで、ログファイルの出力先フォルダーを指定できます。



