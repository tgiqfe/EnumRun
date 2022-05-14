# スクリプトファイル

設定ファイルで指定した``FilesPath``フォルダー配下に、実行させたいスクリプトファイルを配置します。  
このときのスクリプトファイルのファイル名は、以下のルールに従ってください。
- ファイル名の先頭は、数字1～3桁
- その次の文字は「_ (アンダーバー)]
- その次に、任意の文字、任意の文字数でスクリプトファイルの名前
- オプション指定する場合は、拡張子直前の「. (ドット)」の前に、[]で囲んだオプション記号
- 拡張子 (.●●●●)

ファイル名の例
    01_SampleScriptFile01.bat
    02_SampleScriptFile02.bat
    03_SampleScriptFile[w].bat
    04_SampleScriptFile[dpo].bat
