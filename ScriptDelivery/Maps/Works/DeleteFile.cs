
namespace ScriptDelivery.Maps.Works
{
    public class DeleteFile
    {
        /// <summary>
        /// ダウンロードした後にローカル側で削除するファイル/フォルダーのパス。
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("target")]
        public string[] DeleteTarget { get; set; }

        /// <summary>
        /// 削除対象外ファイル/フォルダーのパス。Targetの値とExcludeの値で重複した場合、Exclude側を優先。
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("exclude")]
        public string[] DeleteExclude { get; set; }
    }
}
