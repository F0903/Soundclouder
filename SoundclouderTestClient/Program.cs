using Microsoft.Extensions.Configuration;
using Soundclouder;

using System.Reflection;

var config = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

var client = new SearchClient(new ClientInfo { ClientId = config["client_id"], UserId = config["user_id"] });
var result = await client.SearchAsync("odesza love letter");
var first = result.First();
await first.DownloadAsync("./out.ogg");