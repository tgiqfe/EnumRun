
namespace ScriptDelivery.Maps.Works
{
    public class DeleteFile
    {
        /// <summary>
        /// ダウンロードした後にローカル側で削除するファイル/フォルダーのパス。
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "target")]
        public string[] DeleteTarget { get; set; }

        /// <summary>
        /// 削除対象外ファイル/フォルダーのパス。Targetの値とExcludeの値で重複した場合、Exclude側を優先。
        /// </summary>
        [YamlDotNet.Serialization.YamlMember(Alias = "exclude")]
        public string[] DeleteExclude { get; set; }
    }
}
