
namespace ScriptDelivery.Maps.Works
{
    /// <summary>
    /// ダウンロードした後に、ローカル側のファイルを削除する動作
    /// </summary>
    public enum DeleteAction
    {
        None = 0,               //  何もしない(何も削除しない)
        Indivisual = 1,         //  個別に指定
        ExceptDownload = 2,     //  ダウンロードしたファイル/フォルダー以外を全削除
    }
}
