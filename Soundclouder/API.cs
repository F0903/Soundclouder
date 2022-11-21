using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Soundclouder.Logging;

namespace Soundclouder;

public class MediaNotFoundException : Exception
{
    public MediaNotFoundException(string? message = null) : base(message ?? "No media was found.") { }
}

public enum ResolveKind
{
    Track,
    Playlist
}

public record class ResolveResult(ResolveKind Kind);
public record class TrackResolveResult : ResolveResult
{
    public TrackResolveResult() : base(ResolveKind.Track)
    { }

    public required Track Track { get; init; }
}

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

    public static async Task<TrackResolveResult> ResolveTrackAsync(JsonElement docRoot, string clientId)
    {
        var track = await CreateTrackAsync(docRoot, clientId);
        return new TrackResolveResult { Track = track };
    }

    public static Task<ResolveResult> ResolvePlaylistAsync(JsonElement docRoot)
    {
        //TODO
        throw new NotImplementedException();
    }

    public static async Task<ResolveResult> ResolveAsync(string url, string clientId)
    { 
        const string baseUrl = "https://api-v2.soundcloud.com/resolve";
        using var response = await client.GetAsync($"{baseUrl}?url={url}&client_id={clientId}&app_locale=en");
        response.EnsureSuccessStatusCode();

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        var root = doc.RootElement;
        var kind = root.GetProperty("kind").GetString()!;
        var enumKind = kind switch
        {
            "track" => ResolveKind.Track,
            "playlist" => ResolveKind.Playlist,
            _ => throw new UnreachableException()
        };

        return enumKind switch
        {
            ResolveKind.Track => await ResolveTrackAsync(root, clientId),
            ResolveKind.Playlist => await ResolvePlaylistAsync(root),
            _ => throw new UnreachableException()
        };
    }

    public static async Task<string> GetStreamURLAsync(string clientId, Track media)
    {
        if (mediaStreamCache.TryGetValue(media.ID, out var streamUrl))
            return streamUrl!;

        string url = $"{media.BaseStreamURL}?client_id={clientId}&track_authorization={media.TrackAuth}";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        streamUrl = doc.RootElement.GetProperty("url").GetString()!;
        mediaStreamCache[media.ID] = streamUrl;
        return streamUrl;
    }

    internal static ValueTask<Track> CreateTrackAsync(JsonElement info, string clientId)
    {
        var transcodings = info.GetProperty("media").GetProperty("transcodings");
        var trackAuth = info.GetProperty("track_authorization").GetString();
        var baseStreamUrl = transcodings.EnumerateArray().ElementAtOr(2, transcodings[0]).GetProperty("url").GetString();
        var track = new Track(trackAuth!, baseStreamUrl!)
        {
            ID = info.GetProperty("id").GetUInt64(),
            Title = info.GetProperty("title").GetString()!,
            Author = info.GetProperty("user").GetProperty("username").GetString()!,
            Genre = info.GetProperty("genre").GetString()!,
            Duration = TimeSpan.FromMilliseconds(info.GetProperty("duration").GetUInt64()),
            ClientId = clientId,
            ArtworkUrl = info.GetProperty("artwork_url").GetString()!,
            CommentCount = info.GetProperty("comment_count").GetUInt64(),
            LikesCount = info.GetProperty("likes_count").GetUInt64(),
            LabelName = info.GetProperty("label_name").GetString()!,
            PermaLink = info.GetProperty("permalink_url").GetString()!,
            ReleaseDate = info.GetProperty("release_date").GetString()!,
            RepostsCount = info.GetProperty("reposts_count").GetUInt64(),
            WaveformUrl = info.GetProperty("waveform_url").GetString()!,
        };
        return ValueTask.FromResult(track);
    }

    internal static async Task InsertTrackAsync(ICollection<Track> collection, JsonElement info, string clientId)
    {
        var track = await CreateTrackAsync(info, clientId);
        collection.Add(track); 
    }

    internal static Task InsertPlaylistAsync(ICollection<Track> collection, JsonElement info, string clientId)
    {
        return Task.CompletedTask;
        //TODO

        //var tasks = new List<Task>();
        //var tracks = info.GetProperty("tracks");
        //for (int i = 0; i < tracks.GetArrayLength(); i++)
        //{
        //    var track = tracks[i];
        //    var task = InsertTrackAsync(collection, ref track, clientInfo);
        //    tasks.Add(task);
        //}
        //return Task.WhenAll(tasks);
    }

    public static async Task<SearchResult> SearchAsync(string clientId, string query, int searchLimit = 3)
    {
        Log.Info($"Searching for {query}...");

        query = query.MakeStringURLFriendly();

        const string baseUrl = "https://api-v2.soundcloud.com/search";
        string url = $"{baseUrl}?q={query}&client_id={clientId}&limit={searchLimit}&app_locale=en";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Log.Info($"Found media!");

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        var collection = doc.RootElement.GetProperty("collection");
        var len = collection.GetArrayLength();

        if (len < 1)
        {
            throw new MediaNotFoundException();
        }

        List<Track> tracks = new();
        for (int i = 0; i < len; i++)
        {
            var info = collection[i];

            var kind = info.GetProperty("kind").GetString();
            switch (kind)
            {
                case "track":
                    await InsertTrackAsync(tracks, info, clientId);
                    break;
                case "playlist":
                    await InsertPlaylistAsync(tracks, info, clientId);
                    break;
                default:
                    continue;
            }
        }
        return new SearchResult { ReturnedMedia = tracks };
    }
}