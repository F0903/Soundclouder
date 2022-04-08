using Soundclouder;

var client = new SearchClient(new ClientInfo { ClientId = "tvr5oyEDbwmNuQSmuNFkGLFrMn5wqT3H", UserId = "823336-646462-507678-448882" });
var result = await client.SearchAsync("tramper torben jødebussen");
var first = result.First();
await first.DownloadAsync("./out.mp3");