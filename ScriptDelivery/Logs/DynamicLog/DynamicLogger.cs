using LiteDB;
using System.IO;
using ScriptDelivery.Lib;
using System.Text;

namespace ScriptDelivery.Logs.DynamicLog
{
    internal class DynamicLogger
    {
        private string _logDir = null;
        private LiteDatabase _liteDB = null;
        private Dictionary<string, DynamicLogSession> _sessions = null;

        private StreamWriter _writer = null;
        private bool _logAppend { get { return true; } }
        private bool _writed = false;
        private AsyncLock _lock = null;



        private Dictionary<string, ILiteCollection<BsonDocument>> _collections = null;



        public DynamicLogger(Setting setting)
        {
            _logDir = setting.GetDynamicLogsPath();

            string today = DateTime.Now.ToString("yyyyMMdd");
            string dbPath = Path.Combine(
                _logDir,
                $"DynamicLog_{today}.db");
            _liteDB = new LiteDatabase($"Filename={dbPath};Connection=shared");

            _sessions = new Dictionary<string, DynamicLogSession>();

            //  定期的にセッションを閉じる
            CloseSessionAsync();
        }

        /*
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
        */

        /*
        private LiteDatabase GetLiteDB(string preName)
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string dbPath = Path.Combine(
                _logDir,
                $"{preName}_{today}.db");
            return new LiteDatabase($"Filename={dbPath};Connection=shared");
        }
        */

        public async void Write(string table, Stream bodyStream)
        {
            if (string.IsNullOrEmpty(table)) { return; }
            try
            {
                var session = GetLogSession(table);
                using (await session.Lock.LockAsync())
                {
                    using (var sr = new StreamReader(bodyStream))
                    {
                        var bsonValue = JsonSerializer.Deserialize(sr);
                        BsonDocument doc = bsonValue as BsonDocument;
                        session.Collection.Insert(doc);
                        await session.Writer.WriteLineAsync(doc.ToString());
                    }
                    session.WriteTime = DateTime.Now;
                }
            }
            catch { }
        }

        private DynamicLogSession GetLogSession(string table)
        {
            try
            {
                return _sessions[table];
            }
            catch
            {
                _sessions[table] = new DynamicLogSession(table, _logDir, _liteDB);
                return _sessions[table];
            }
        }


        public async void Write2(string table, Stream bodyStream)
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

        private async void CloseSessionAsync()
        {
            while (true)
            {
                await Task.Delay(60 * 1000);
                var keys = _sessions.
                    Where(x => (DateTime.Now - x.Value.WriteTime).TotalSeconds > 60).
                    Select(x => x.Key);
                foreach (string key in keys)
                {
                    using (await _sessions[key].Lock.LockAsync())
                    {
                        _sessions[key].Writer.Dispose();
                        _sessions[key].Collection = null;
                    }
                    _sessions.Remove(key);
                }
            }
        }

        public async void Close()
        {
            foreach (var session in _sessions.Values)
            {
                using(await session.Lock.LockAsync())
                {
                    session.Writer.Dispose();
                    session.Collection = null;
                }
            }
        }
    }
}
