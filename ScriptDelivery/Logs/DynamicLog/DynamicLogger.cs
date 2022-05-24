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

        private bool _writed = false;

        public DynamicLogger(Setting setting)
        {
            string logFileName =
                $"DynamicLog_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetCynamicLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetCynamicLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _rwLock = new ReaderWriterLock();

            _liteDB = GetLiteDB("DynamicLog");
            _collections = new Dictionary<string, ILiteCollection<BsonDocument>>();

            //  定期的にログファイルを書き込むスレッドを開始
            WriteInFile(logPath);
        }

        public async void Write(string table, Stream bodyStream)
        {
            try
            {
                _rwLock.AcquireWriterLock(10000);
                
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

                    await _writer.WriteLineAsync(doc);
                    _writed = true;
                }
            }
            catch { }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// 定期的にログをファイルに書き込む
        /// </summary>
        /// <param name="logPath"></param>
        private async void WriteInFile(string logPath)
        {
            while (true)
            {
                await Task.Delay(60 * 1000);
                if (_writed)
                {
                    try
                    {
                        _rwLock.AcquireWriterLock(10000);
                        _writer.Dispose();
                        _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
                    }
                    catch { }
                    finally
                    {
                        _writed = false;
                        _rwLock.ReleaseWriterLock();
                    }
                }
            }
        }

        /// <summary>
        /// クローズ処理
        /// </summary>
        public override void Close()
        {
            base.Close();
        }
    }
}
