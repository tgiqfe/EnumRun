using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Runtime.InteropServices;

namespace ScriptDelivery
{
    public class Setting
    {
        public string MapsPath { get; set; }
        public string FilesPath { get; set; }
        public string LogsPath { get; set; }
        public string MinLogLevel { get; set; }

        public string DynamicLogsPath { get; set; }
        public ParamSyslog Syslog { get; set; }

        #region Syslog

        public class ParamSyslog
        {
            /// <summary>
            /// ログ転送先サーバ(Syslog)のサーバ
            /// 記述例⇒udp://192.168.10.100:514
            /// </summary>
            public string Server { get; set; }
            public string Facility { get; set; }
            public string Format { get; set; }
            public bool? SslEncrypt { get; set; }
            public int? SslTimeout { get; set; }
            public string SslCertFile { get; set; }
            public string SslCertPassword { get; set; }
            public string SslCertFriendryName { get; set; }
            public bool? SslIgnoreCheck { get; set; }

            public override string ToString()
            {
                return string.Format(
                    "[ Server={0} Facility={1} Format={2} SslEncrypt={3} SslTimeout={4} SslCertFile={5} SslCertPassword={6} SslCertFriendryName={7} SslIgnoreCheck={8} ]",
                    this.Server,
                    this.Facility,
                    this.Format,
                    this.SslEncrypt,
                    this.SslTimeout,
                    this.SslCertFile,
                    this.SslCertPassword,
                    this.SslCertFriendryName,
                    this.SslIgnoreCheck);
            }
        }

        #endregion

        public void Init()
        {
            this.MapsPath = "bin/store/maps";
            this.FilesPath = "bin/store/files";
            this.LogsPath = "bin/store/logs";
            this.MinLogLevel = "info";
            this.DynamicLogsPath = "bin/dynamicLogs";
            this.Syslog = new ParamSyslog()
            {
                Server = null,
                Facility = "user",
                Format = "RFC3164",
                SslEncrypt = false,
                SslTimeout = 3000,
                SslCertFile = null,
                SslCertPassword = null,
                SslCertFriendryName = null,
                SslIgnoreCheck = false
            };
        }

        public void ChangePath()
        {
            switch (Item.Platform)
            {
                case Platform.Windows:
                    this.MapsPath = MapsPath.Replace("/", "\\");
                    this.FilesPath = FilesPath.Replace("/", "\\");
                    break;
                case Platform.Linux:
                case Platform.MacOSX:
                    this.MapsPath = MapsPath.Replace("\\", "/");
                    this.FilesPath = FilesPath.Replace("\\", "/");
                    break;
            }
        }

        public string GetLogsPath()
        {
            return string.IsNullOrEmpty(this.LogsPath) ?
                Path.Combine(Item.ExecDirectoryPath, "Logs") :
                ExpandEnvironment(this.LogsPath);
        }

        public string GetDynamicLogsPath()
        {
            return string.IsNullOrEmpty(this.DynamicLogsPath) ?
                Path.Combine(Item.ExecDirectoryPath, "DynamicLogs") :
                ExpandEnvironment(this.DynamicLogsPath);
        }

        private string ExpandEnvironment(string text)
        {
            for (int i = 0; i < 5 && text.Contains("%"); i++)
            {

                text = Environment.ExpandEnvironmentVariables(text);
            }
            return text;
        }

        #region Serialize/Deserialize

        public static Setting Deserialize(string filePath)
        {
            Setting setting = null;
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    setting = JsonSerializer.Deserialize<Setting>(sr.ReadToEnd(),
                        new JsonSerializerOptions()
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            //IgnoreReadOnlyProperties = true,
                            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            //WriteIndented = true,
                            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                        });
                }
            }
            catch { }
            if (setting == null)
            {
                setting = new Setting();
                setting.Init();
            }

            return setting;
        }

        public void Serialize(string filePath)
        {
            if (filePath.Contains(Path.DirectorySeparatorChar))
            {
                string parent = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
            }
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(this,
                         new JsonSerializerOptions()
                         {
                             Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                             //IgnoreReadOnlyProperties = true,
                             //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                             WriteIndented = true,
                             Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                         });
                    sw.WriteLine(json);
                }
            }
            catch { }
        }

        #endregion
    }
}
