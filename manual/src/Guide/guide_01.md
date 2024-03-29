# 実行環境

## システム要件

### 推奨環境(諸注意)

1. 実行するユーザーは、管理者権限(Administratorsグループに所属)であること。
2. ユーザーアカウント制御が「通知しない」に設定されていること。
3. ネットワークに接続していること。
4. ドメイン参加PCの場合、GPOでループバックポリシーオプションを使用しないこと。
5. ドメイン参加PCの場合、Active Directoryドメインコントローラと通信可能なこと。

<br>
[1.][2.] について  
``RunAsAdmin``オプション([a]オプション)を使用しない場合は問題ありません。

[3.] について  
``DGReachableOnly``オプション([p]オプション)を使用しない場合は問題ありません。

[4.] について  
ループバックポリシーオプションを使用する場合、主にログオンスクリプト等が、2回実行されてしまいます。  
設定ファイルの``RestTime``パラメータで十分な数値 (60秒程度であればほぼ問題ありません) を設定しておけば、問題は発生しにくいです。  

[5.] について  
ドメイン情報取得の際に、ドメインコントローラから情報を取得することがあります。  
ドメインコントローラと通信不可の場合、タイムアウト時間までプロセスが停止することがあります。




## 実行ファイル

### ダウンロード

``EnumRun.exe``ファイル。

GitHubから最新バージョンの``.zip``ファイルをダウンロード。  
[EnumRun/GitHub](https://github.com/tgiqfe/EnumRun/releases/latest)

ダウンロードしたファイルを解凍し、任意の場所に保存します。  
※このとき、セキュリティブロックが有効になっている場合があるので、ファイルのプロパティを確認して、「許可する」にチェックONしておくことをお勧めします。

★ここにファイルのプロパティの画像を

### ファイル名変更

ダウンロードしたファイルを任意の名前に変更します。  
変更したファイル名はProcessNameとして、実行時に毎回参照します。  
※デフォルトでは、以下のProcessNameの為のRange設定がされています。ダウンロードした``EnumRun.exe``ファイルをコピーして4つになるまで増やし、以下のような名前に変更することをお勧めします。
- StartupScript.exe
- ShutdownScript.exe
- LogonScript.exe
- LogoffScript.exe

★ここにエクスプローラ上で4ファイルを用意した状態の画像を















