using Microsoft.Extensions.Configuration;
using Soundclouder;
using Soundclouder.Logging;

using System.Reflection;

var config = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Log.Handler += (sev, msg) => Console.WriteLine($"[{sev.ToString().ToUpper()}] {msg}");

var client = new SearchClient(new ClientInfo { ClientId = config["client_id"], UserId = config["user_id"] });
var result = await client.SearchAsync("odesza love letter");
var first = result.First();
await first.DownloadAsync("./out.ogg");