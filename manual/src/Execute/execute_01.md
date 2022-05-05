# EnumRun実行

## 自動実行用の設定

以下の方法で自動実行するように設定が可能です。  
それぞれの設定方法は、ここでは割愛します。

★⇒推奨の設定方法

:::tip
スタートアップスクリプト
- ローカルのグループポリシーオブジェクト [スタートアップ]★
- ドメインのグループポリシーオブジェクト [スタートアップ]
- タスクスケジューラ
- (別ソフトを使用する必要有り)Windowsサービスの起動時(OnStart時)
:::
:::tip
ログオンスクリプト
- ローカルのグループポリシーオブジェクト [ログオン]★
- ドメインのグループポリシーオブジェクト [ログオン]
- ``C:\ProgramData\Microsoft\Windows\Start Menu\Programs\StartUp``に配置★
- ``%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup``に配置
- ``HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run``に記述
- ``HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce``に記述
- ``HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run``に記述
- ``HKEY_CURRENT_USER\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce``に記述
- ``HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run``に記述
- ``HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce``に記述
- ``HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run``に記述
- ``HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\RunOnce``に記述
- タスクスケジューラ
:::
:::tip
ログオフスクリプト
- ローカルのグループポリシーオブジェクト [ログオフ]★
- ドメインのグループポリシーオブジェクト [ログオフ]
- (別ソフトを使用する必要有り)ログオフイベント発生時
:::
:::tip
シャットダウンスクリプト
- ローカルのグループポリシーオブジェクト [シャットダウン]★
- ドメインのグループポリシーオブジェクト [シャットダウン]
- (別ソフトを使用する必要有り)Windowsサービスの起動時(OnStop時)
- (別ソフトを使用する必要有り)シャットダウンイベント発生時
:::







