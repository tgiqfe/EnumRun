# サンプル(設定ファイル)

## デフォルトパターン

## 基本パターン

### パターン1

ほぼ最小限の構成。
- ファイル、ログ、標準出力の各ファイル保存先は``%TEMP%\EnumRun``配下
- Logstash、Syslog等へのログ転送は無し
- 連続起動禁止期間⇒60秒
- ログの保持期間⇒10日
- 最小ログレベル⇒Info

<code-group>
<code-block title="JSON" active>

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

</code-block>
<code-block title="Yml">

```yml
setting:
  restTime: 60
  detentionPeriod: 10
  minLogLevel: Info
  ranges:
    StartupScript: 0-9
    LogonScript: 11-29
    LogoffScript: 81-89
    ShutdownScript: 91-99
```

</code-block>
<code-block title="Text">

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

</code-block>
</code-group>

## Logstashログ転送パターン


## Syslogログ転送パターン

### パターン1

UDPで転送

### パターン2

TCPで転送

### パターン3

暗号化TCPで転送(クライアント認証無し)

### パターン4

暗号化TCPで転送(クライアント認証有り)





