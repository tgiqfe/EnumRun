using LiteDB;
using System.IO;
using ScriptDelivery.Lib;
using System.Text;

namespace ScriptDelivery.Logs.DynamicLog
{
    internal class DynamicLogReceiver
    {
        private string _logDir = null;
        private LiteDatabase _liteDB = null;
        private Dictionary<string, DynamicLogSession> _sessions = null;

        public DynamicLogReceiver(Setting setting)
        {
            _logDir = setting.GetDynamicLogsPath();

            string today = DateTime.Now.ToString("yyyyMMdd");
            string dbPath = Path.Combine(
                _logDir,
                $"DynamicLog_{today}.db");
            _liteDB = new LiteDatabase($"Filename={dbPath};Connection=shared");

            _sessions = new Dictionary<string, DynamicLogSession>();

            //  定期的にセッションを閉じる
            CloseSequenseAsync();
        }

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

        /// <summary>
        /// 定期的にセッションを閉じる
        /// </summary>
        private async void CloseSequenseAsync()
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
                    Console.WriteLine($"{key} close ########");
                    _sessions.Remove(key);
                }
            }
        }

        /// <summary>
        /// クローズ処理
        /// </summary>
        public async Task CloseAsync()
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
