# ログ転送(Syslogサーバ側設定)

Linux前提。  
SyslogのWindows Serverは割愛。

## 必要パッケージ

- rsyslog
- gnutls-utils ※TLS暗号化する場合のみ
- rsyslog-gnutls ※TLS暗号化する場合のみ

``rsyslog``は恐らくOSインストール時に同時インストールされる為、気にしなくてOK。

## 各通信用設定

### UDP

コンフィグファイル``/etc/rsyslog.conf``を、以下のように変更。

設定箇所 (変更前)
```bash
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所 (変更後)
```bash{2-3}
# provides UDP syslog reception
module(load="imudp")
input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

### TCP(暗号化無し)

コンフィグファイル``/etc/rsyslog.conf``を、以下のように変更。

設定箇所 (変更前)
```bash
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所 (変更後)
```bash{6-7}
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
module(load="imtcp")
input(type="imtcp" port="514")
```

### TCP(TLS暗号化、クライアント認証無し)

事前に必要パッケージをインストール

Ubuntu
```bash
apt-get install gnutls-utils
apt-get install rsyslog-gnutls
```

CentOS
```bash
dnf install gnutls-utils
dnf install rsyslog-gnutls
```

コンフィグファイル``/etc/rsyslog.conf``を、以下のように変更。

設定箇所 (変更前)
```bash
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所 (変更後)
```bash{7-20}
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
module(
  load="imtcp"
  StreamDriver.Name="gtls"
  StreamDriver.Mode="1"
  StreamDriver.Authmode="anon"
)
input(type="imtcp" port="514")

global(
  DefaultNetstreamDriver="gtls"
  DefaultNetstreamDriverCAFile="/etc/rsyslog.d/tls/rootCA.crt"
  DefaultNetstreamDriverCertFile="/etc/rsyslog.d/tls/cert.crt"
  DefaultNetstreamDriverKeyFile="/etc/rsyslog.d/tls/cert.key"
)
```

::: tip
``StreamDriver.Authmode``の値を``anon``に設定。
:::

### TCP(TLS暗号化、クライアント認証有り)

事前に必要パッケージをインストール  
⇒ [TCP暗号化(TLS暗号化、クライアント認証無し)](#tcp暗号化-tls暗号化、クライアント認証無し)を参照

コンフィグファイル``/etc/rsyslog.conf``を、以下のように変更。

設定箇所 (変更前)
```bash
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
#input(type="imtcp" port="514")
```

設定箇所 (変更後)
```bash{7-21}
# provides UDP syslog reception
#module(load="imudp")
#input(type="imudp" port="514")

# provides TCP syslog reception
#module(load="imtcp")
module(
  load="imtcp"
  StreamDriver.Name="gtls"
  StreamDriver.Mode="1"
  StreamDriver.Authmode="x509/name"
  PermittedPeer=["クライアント証明書のCN名"]
)
input(type="imtcp" port="514")

global(
  DefaultNetstreamDriver="gtls"
  DefaultNetstreamDriverCAFile="/etc/rsyslog.d/tls/ca.crt"
  DefaultNetstreamDriverCertFile="/etc/rsyslog.d/tls/cert.crt"
  DefaultNetstreamDriverKeyFile="/etc/rsyslog.d/tls/cert.key"
)
```

::: tip
``StreamDriver.Authmode``の値を``x509/name``に設定。
:::

::: tip
``PermittedPeer``の値は、クライアント証明書のCN名 (FQDN必須) に合わせる必要有り。
:::

暗号化通信うる場合は、ルート証明書(必要に応じて中間証明書も含む)を、Syslogクライアントに予めインストールしておく必要有り。


## 出力用設定

### ファシリティごとに対象外設定

初期状態は、``/etc/rsyslog.conf``もしくは、``/etc/rsyslog.d/配下の*.conf``のどこかに記述されている、
```
*.*;auth,authpriv.none              -/var/log/syslog
```

の設定により、アプリケーションが送ったSyslogデータは全て``/var/log/syslog``ファイルにまとめられて、闇鍋になってします。
(ディストリビューションにより、出力先ファイルの名前が異なる可能性有り)  

事前に決めたファシリティを、闇鍋対象がにする為には、  
(ファシリティ``local0``を対象外にする場合)
```
*.*;auth,authpriv.none,local0.none              -/var/log/syslog
```

のように設定。

::: tip
ファイル名の前の「- (ハイフン)」は、非同期書き込みするという設定。  
「-」無しの場合は同期書き込み。  
非同期書き込みのほうがパフォーマンスが良いとのこと。
:::

### 対象ファシリティを特定のファイルに設定

``/etc/rsyslog.conf``もしくは、``/etc/rsyslog.d/配下の*.conf``のファイルに、以下のように追記する。  
(ファシリティ``local0``を対象外にする場合)
```
local0.debug            -/var/log/example.log
```

何かあった時にファイル名変更や削除で容易に設定無効化ができる為、``/etc/rsyslog.c``配下に、新規ファイルを作成して設定を記述することをお勧めします。  

### 設定ファイルの構文チェック

以下のように実行。
```bash
# /etc/rsyslog.conf をチェック
rsyslogd -N 1

# /etc/rsyslog.d 配下の10-sample.conf をチェック
rsyslogd -N 1 -f /etc/rsyslog.d/10-sample.conf
```

構文に問題が無い場合は、以下のように出力される。
```
rsyslogd: version 8.2001.0, config validation run (level 1), master config /etc/rsyslog.d/60-test.conf
rsyslogd: End of config validation run. Bye.
```


::: tip
Syslogデータの出力先を設定した後、ログローテーションに設定を追加することも忘れずに。
:::

