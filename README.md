# Soundclouder

A minimal asynchronous client for the SoundCloud API which requires no Auth Key.
You only need a user_id and a client_id, both of which can be gotten by inspecting intercepted requests under the "network" tab in the developer window from any browser.

Currently, the library is very primitive, and does not currently support things like playlists (although planned).

### Examples

#### Basic Stream URL
```cs
using Soundclouder;

var clientInfo = new ClientInfo { ClientId = "*your client id*", UserId = "*your user id*" };
var client = new SearchClient(clientInfo);

var searchResult = await client.SearchAsync("*your search query*");
var media = searchResult.First(); // Filter your result.

var url = await media.GetStreamURLAsync();
```
It's that easy.

#### Download and Conversion
**(Requires [FFmpeg.exe](https://ffmpeg.org/download.html) in library dll directory!)**
```cs
using Soundclouder;

var clientInfo = new ClientInfo { ClientId = "*your client id*", UserId = "*your user id*" };
var client = new SearchClient(clientInfo);

var searchResult = await client.SearchAsync("*your search query*");
var media = searchResult.First(); // Filter your result.

await media.DownloadAsync("./out.mp3"); // You can use any path and extension, and ffmpeg will convert automatically.
```
Nothing more needed.

### Logging

The library also includes optional logging output via Soundclouder.Logging.Log.
To use this, simply set the Handler property to a method of your choice.
```cs
using Soundclouder.Logging;

Log.Handler += (severity, message) => Console.WriteLine($"[{severity.ToString().ToUpper()}] {message}");
```
Done!
