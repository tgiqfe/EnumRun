
namespace ScriptDelivery.Logs.DynamicLog
{
    internal class DynamicLogSession
    {
        public string Table { get; set; }
        public string FileName { get; set; }
        public LiteDB.ILiteCollection<LiteDB.BsonDocument> Collection { get; set; }
        public System.IO.StreamWriter Writer { get; set; }
        public ScriptDelivery.Lib.AsyncLock Lock { get; set; }
        public DateTime WriteTime { get; set; }

        public DynamicLogSession(string table, string logDir, LiteDB.LiteDatabase liteDB)
        {
            this.Table = table;
            string today = DateTime.Now.ToString("yyyyMMdd");
            this.FileName = System.IO.Path.Combine(logDir, $"{table}_{today}.log");
            this.Collection = liteDB.GetCollection(table);
            this.Writer = new System.IO.StreamWriter(FileName, true, System.Text.Encoding.UTF8);
            this.Lock = new ScriptDelivery.Lib.AsyncLock();
        }
    }
}
