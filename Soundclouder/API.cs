using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Soundclouder.Logging;

namespace Soundclouder;

public static class API
{
    static readonly HttpClient client = new();

    public const int MaxCachedStreams = 50;
    internal static MediaStreamCache mediaStreamCache = new(MaxCachedStreams);

    static T ElementAtOr<T>(this IEnumerable<T> col, Index index, T or)
    {
        try
        {
            return col.ElementAt(index);
        }
        catch (IndexOutOfRangeException)
        {
            return or;
        }
    }

    static Task<JsonDocument> ReadAsJsonDocumentAsync(this HttpContent content)
    {
        return content.ReadAsStreamAsync().ContinueWith(x => JsonDocument.Parse(x.Result));
    }

    public static ValueTask ClearMediaStreamCache()
    {
        mediaStreamCache.Clear();
        return ValueTask.CompletedTask;
    }

    internal static async Task<string> GetStreamURLAsync(ClientInfo clientInfo, Media media)
    {
        if (mediaStreamCache.TryGetValue(media.ID, out var streamUrl))
            return streamUrl!;

        string url = $"{media.BaseStreamURL}?client_id={clientInfo.ClientId}&user_id={clientInfo.UserId}&track_authorization={media.TrackAuth}";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        streamUrl = doc.RootElement.GetProperty("url").GetRawText();
        mediaStreamCache[media.ID] = streamUrl;
        return streamUrl;
    }

    internal static async Task<SearchResult> SearchAsync(ClientInfo clientInfo, string query, int searchLimit = 3)
    {
        Log.Info($"Searching for {query}...");

        query = query.MakeStringURLFriendly();

        string url = $"https://api-v2.soundcloud.com/search?q={query}&user_id={clientInfo.UserId}&client_id={clientInfo.ClientId}&limit={searchLimit}&app_locale=en";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Log.Info($"Found media!");

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        var collection = doc.RootElement.GetProperty("collection");
        var len = collection.GetArrayLength();

        //TODO: Support playlists/albums.
        List<Media> result = new();
        for (int i = 0; i < len; i++)
        {
            var info = collection[i];

            var kind = info.GetProperty("kind").GetString();
            if (kind != "track") continue;

            var transcodings = info.GetProperty("media").GetProperty("transcodings");
            var media = new Media
            {
                ID = info.GetProperty("id").GetUInt64(),
                Title = info.GetProperty("title").GetString()!,
                Author = info.GetProperty("user").GetProperty("username").GetString()!,
                Genre = info.GetProperty("genre").GetString()!,
                TrackAuth = info.GetProperty("track_authorization").GetString()!,
                BaseStreamURL = transcodings.EnumerateArray().ElementAtOr(2, transcodings[0]).GetProperty("url").GetString()!,
                ClientInfo = clientInfo,
            };
            result.Add(media);
        }
        return new SearchResult { ReturnedMedia = result };
    }
}