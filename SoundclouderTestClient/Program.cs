using Microsoft.Extensions.Configuration;
using Soundclouder;
using Soundclouder.Logging;

using System.Reflection;

var config = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Log.Handler += (sev, msg) => Console.WriteLine($"[{sev.ToString().ToUpper()}] {msg}");

var clientId = config["client_id"];
if (clientId is null) throw new NullReferenceException("Please set client_id in project User Secrets"); 

var client = new SearchClient(new ClientInfo { ClientId = clientId });
var result = await client.SearchAsync("odesza love letter");
var first = result.First();
await first.DownloadAsync("./out.ogg");