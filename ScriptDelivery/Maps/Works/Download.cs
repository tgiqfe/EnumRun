using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Diagnostics;

namespace ScriptDelivery.Maps.Works
{
    internal class Download
    {
        [YamlMember(Alias = "source")]
        public string Source { get; set; }

        [YamlMember(Alias = "destination")]
        public string Destination { get; set; }

        [YamlMember(Alias = "keep")]
        public string Keep { get; set; }

        [YamlMember(Alias = "user")]
        public string UserName { get; set; }

        [YamlMember(Alias = "password")]
        public string Password { get; set; }

        public bool GetKeep()
        {
            return this.Keep == null ?
                false :
                new string[]
                {
                    "", "0", "-", "false", "fals", "no", "not", "none", "non", "empty", "null", "否", "不", "無", "dis", "disable", "disabled"
                }.All(x => !x.Equals(this.Keep, StringComparison.OrdinalIgnoreCase));
        }
    }
}

//  ダウンロード先は、どの場合でもローカル側の特定フォルダー配下になるはずなので、
//  ここに明記する必要はなさそう。
//  Destinationを削除し、SourceをPathに変更

//  相対パスである前提で記述してほしいけれど、もし絶対パスを記述された場合の対処も検討要

//  ダウンロードするのみで、削除させる場合の処理も検討する必要有り。
//  元々Smbでネットワーク越しのrobocopy /MIRの予定だった為、Httpの場合は検討が足りていない。
