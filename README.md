# Soundclouder

A minimal asynchronous client for the SoundCloud API which requires no Auth Key.
You only need a client_id, which can be gotten by inspecting intercepted requests from SoundCloud under the "network" tab in the developer window from any browser.

Currently, the library is very primitive.

It also includes a convenient test console app, that can be used standalone to download tracks and playlists from SoundCloud.

### Examples

#### Basic Stream URL
```cs
using Soundclouder;

var client = new SearchClient("*your client id*");

var searchResult = await client.SearchAsync("*your search query*");
var media = searchResult.First(); // Filter your result.

var url = await media.GetStreamURLAsync();
```

#### Download and Conversion
**(Requires [FFmpeg.exe](https://ffmpeg.org/download.html) in app directory or PATH!)**
```cs
using Soundclouder;

var client = new SearchClient("*your client id*");

var searchResult = await client.SearchAsync("*your search query*");
var media = searchResult.First(); // Filter your result.

await media.DownloadAsync("./out.mp3"); // You can use any path and extension, and ffmpeg will convert automatically.
```

### Logging

The library also includes very basic optional logging output via Soundclouder.Logging.Log.
To use this, simply set the Handler property to a method of your choice.
```cs
using Soundclouder.Logging;

Log.Handler += (severity, message) => Console.WriteLine($"[{severity.ToString().ToUpper()}] {message}");
```
