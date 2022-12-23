using System.Reflection;

using Microsoft.Extensions.Configuration;

using Soundclouder;
using Soundclouder.Entities;
using Soundclouder.Logging;

using SoundclouderTestClient;

var config = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();

Log.Handler += (sev, msg) => Console.WriteLine($"[{sev.ToString().ToUpper()}] {msg}");

var clientId = config["client_id"];
if (clientId is null) throw new NullReferenceException("Please set client_id in project User Secrets");

var client = new SearchClient(clientId);

while (true)
{
    Console.Write("Enter query: ");
    var query = Console.ReadLine();
    if (query is null) continue;
    IReadOnlyCollection<Track> result;
    try
    {
        result = query.AsSpan().IsUrl() switch
        {
            true => (await client.ResolveAsync(query)).ToTrackList(),
            false => (await client.SearchAsync(query)).ToTrackList(),
        };
    }
    catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("Request returned 401 Unauthorized. Please update client_id\nPress any key to exit...");
        Console.ReadLine();
        return;
    }

    var mediaList = result.ToList();

    if (mediaList.Count < 1)
    {
        Console.WriteLine("No results found :(");
        continue;
    }
    else if (mediaList.Count < 2)
    {
        var media = mediaList.First();
        await DownloadSingle(media);
    }
    else
    {
        for (int i = 0; i < mediaList.Count; i++)
        {
            var item = mediaList[i];
            Console.WriteLine($"Found [{i}] {item.Title}");
        }
        const int maxAttempts = 3;
        int tries = 0;
        while (true)
        {
            Console.WriteLine("""

            Which media would to like to download?
            Options:
            all: Downloads all
            *index*: Downloads specified media from output list. 

            """); 
            var reply = Console.ReadLine();
            if (reply is null) break;
            if (reply.AsSpan().IsAlphabetic() && reply.ToLower() == "all")
            {
                await DownloadPlaylist(mediaList);
                break;
            }
            else if (reply.AsSpan().IsNumeric())
            {
                if (int.TryParse(reply, out var index))
                {
                    var media = mediaList[index];
                    await DownloadSingle(media);
                    break;
                } 
            }
            ++tries;
            Console.WriteLine($"Invalid input. Try again. ({tries}/{maxAttempts})");
        }
    }
    Console.WriteLine("Done!\nClearing in 3 seconds...");
    await Task.Delay(3000);
    Console.Clear();
}

Task DownloadPlaylist(IReadOnlyCollection<Track> playlist)
{
    IEnumerable<Task> Download()
    {
        foreach (var item in playlist)
        {
            yield return DownloadSingle(item);
        }
    }
    return Task.WhenAll(Download());
}

Task DownloadSingle(Track media) => media.DownloadAsync($"./{media.ID}.ogg");