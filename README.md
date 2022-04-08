# Soundclouder

A minimal asynchronous client for the SoundCloud API which requires no Auth Key.
You only need a user_id and a client_id, both of which can be gotten by inspecting intercepted requests under the "network" tab in the developer window from any browser.

Currently, the library is very primitive, and does not currently support things like playlists (although planned).
You can search for songs as normal, and get a list of search results from which you can filter.
Then you can either choose to get a direct link to the media stream, or you can download it directly by using Media.Download() and making sure to have an FFmpeg executable in the same directory as the library.

### Examples

```cs
using Soundclouder;

var clientInfo = new ClientInfo { ClientId = "*your client id*", UserId = "*your user id*" };
var client = new SearchClient(clientInfo);

var searchResult = await client.SearchAsync("*your search query*");
var media = searchResult.First(); // Filter your result.

media.DownloadAsync("./out.mp3"); // Download and convert your media to your liking!
```

It's that easy.

### Logging

The library also includes optional logging output via Soundclouder.Logging.Log.
To use this, simply set the Handler property to a method of your choice.
```cs
using Soundclouder.Logging;

Log.Handler += (severity, message) => Console.WriteLine($"[{severity.ToString().ToUpper()}] {message}");
```
Done!
