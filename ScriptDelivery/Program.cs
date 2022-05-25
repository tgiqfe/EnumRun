using System;
using System.Text;
using ScriptDelivery;
using ScriptDelivery.Files;

bool debug = true;
if (debug)
{
    var sampleMap = ScriptDelivery.Maps.MappingGenerator.Deserialize("bin\\sample01.txt");
    ScriptDelivery.Maps.MappingGenerator.Serialize(sampleMap, "bin\\sample01.txt");
    ScriptDelivery.Maps.MappingGenerator.Serialize(sampleMap, "bin\\sample01.csv");
    ScriptDelivery.Maps.MappingGenerator.Serialize(sampleMap, "bin\\sample01.json");

    Console.ReadLine();
    Environment.Exit(0);
}


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var options = new System.Text.Json.JsonSerializerOptions()
{
    //Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    IgnoreReadOnlyProperties = true,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    //WriteIndented = true,
    //Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
};

#region Routing

app.MapGet("/", () => "");

app.MapPost("/", () => "");

//  Mapファイルの要求への処理
app.MapPost("/map", async (HttpContext context) =>
{
    string address = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

    Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Info, address, "Post_map", "Send, MapFile");

    context.Response.Headers.Add("App-Version", Item.CurrentVersion);
    context.Response.ContentType = "application/json; charset=utf-8";
    await context.Response.WriteAsync(Item.MappingFileCollection.Content);
});

//  HttpDownloadファイルのリストの要求への処理
app.MapPost("/download/list", async (HttpContext context) =>
{
    string address = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

    if (context.Request.HasJsonContentType())
    {
        Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Info, address, "Post_download_list", "Send, DownloadList");

        List<DownloadHttp> reqList = await context.Request.ReadFromJsonAsync<List<DownloadHttp>>();
        List<DownloadHttp> resList = Item.DownloadFileCollection.RequestToResponse(reqList);
        await context.Response.WriteAsJsonAsync(resList, options);
    }
    else
    {
        Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Warn, address, "Post_download_list", "Invalid post body. Require Json.");
    }
});

//  HttpDownloadファイルを送信
app.MapGet("/download/files", async (HttpContext context) =>
{
    string address = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";

    string fileName = context.Request.Query["fileName"];
    string filePath = Path.Combine(Item.Setting.FilesPath, fileName);

    Item.Logger.Write(ScriptDelivery.Logs.LogLevel.Info, address, "Get_download_files", $"Send, File => {fileName}");

    context.Response.Headers.Add("Content-Disposition", "attachment; filename=" + System.Web.HttpUtility.UrlEncode(fileName));
    await context.Response.SendFileAsync(filePath);
});

//  LogReceive用
app.MapPost("/logs/{table}", (HttpContext context) =>
{
    var syncIOFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
    if (syncIOFeature != null)
    {
        syncIOFeature.AllowSynchronousIO = true;
    }
    var table = context.Request.RouteValues["table"]?.ToString();
    Item.DynamicLogger.Write(table, context.Request.Body);
    return "";
});

#endregion

using (var worker = new SessionWorker())
{
    app.Run();
}


