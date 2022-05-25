namespace ScriptDelivery.Logs.DynamicLog
{
    internal class DynamicLogSession
    {
        public string Table { get; set; }
        public string FileName { get; set; }
        public LiteDB.ILiteCollection<LiteDB.BsonDocument> Collection { get; set; }
        public System.IO.StreamWriter Writer { get; set; }
        public ScriptDelivery.Lib.AsyncLock Lock { get; set; }
    }
}
