using LiteDB;
using System.IO;
using ScriptDelivery.Lib;
using System.Text;

namespace ScriptDelivery.Logs.DynamicLog
{
    internal class DynamicLogger : LoggerBase
    {
        protected override bool _logAppend { get { return true; } }

        private Dictionary<string, ILiteCollection<BsonDocument>> _collections = null;

        public DynamicLogger(Setting setting)
        {
            string logFileName =
                $"DynamicLog_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetDynamicLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetDynamicLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _lock = new AsyncLock();

            _liteDB = GetLiteDB("DynamicLog");
            _collections = new Dictionary<string, ILiteCollection<BsonDocument>>();

            //  定期的にログファイルを書き込むスレッドを開始
            WriteInFile(logPath);
        }

        public async void Write(string table, Stream bodyStream)
        {
            try
            {
                using (await _lock.LockAsync())
                {
                    if (string.IsNullOrEmpty(table))
                    {
                        return;
                    }

                    ILiteCollection<BsonDocument> collection = null;
                    try
                    {
                        collection = _collections[table];
                    }
                    catch
                    {
                        collection = _liteDB.GetCollection(table);
                        _collections[table] = collection;
                    }
                    using (var sr = new StreamReader(bodyStream))
                    {
                        var bsonValue = JsonSerializer.Deserialize(sr);
                        BsonDocument doc = bsonValue as BsonDocument;
                        collection.Insert(doc);

                        await _writer.WriteLineAsync(doc.ToString());
                    }

                    _writed = true;
                }
            }
            catch { }
        }
    }
}
