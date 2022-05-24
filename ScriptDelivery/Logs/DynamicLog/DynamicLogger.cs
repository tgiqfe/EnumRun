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
                $"ScriptDelivery_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string logPath = Path.Combine(setting.GetCynamicLogsPath(), logFileName);
            TargetDirectory.CreateParent(logPath);

            _logDir = setting.GetCynamicLogsPath();
            _writer = new StreamWriter(logPath, _logAppend, Encoding.UTF8);
            _rwLock = new ReaderWriterLock();

            _collections = new Dictionary<string, ILiteCollection<BsonDocument>>();
        }

        public void Write(string table, Stream bodyStream)
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
                }
                using (var sr = new StreamReader(bodyStream))
                {
                    var bsonValue = JsonSerializer.Deserialize(sr);
                    BsonDocument doc = bsonValue as BsonDocument;
                    collection.Insert(doc);
                }
            }
            catch { }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }
    }
}
