using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Soundclouder.Entities;
using Soundclouder.Logging;

using static System.Net.WebRequestMethods;

namespace Soundclouder;

public class MediaNotFoundException : Exception
{
    public MediaNotFoundException(string? message = null) : base(message ?? "No media was found.") { }
}

public static class API
{
    static readonly HttpClient client = new();

    public const int MaxCachedStreams = 50;
    internal static MediaStreamCache mediaStreamCache = new(MaxCachedStreams);

    static Task<JsonDocument> ReadAsJsonDocumentAsync(this HttpContent content)
    {
        return content.ReadAsStreamAsync().ContinueWith(x => JsonDocument.Parse(x.Result));
    } 

    internal static async ValueTask<Playlist> CreatePlaylistAsync(JsonElement element, string clientId)
    {
        var previewTrackElems = element.GetProperty("tracks");
        var trackCount = element.GetProperty("track_count").GetUInt32();
        var trackIds = new string[trackCount];
        for (int i = 0; i < trackCount; i++)
        {
            var track = previewTrackElems[i];
            var id = track.GetProperty("id").GetUInt32().ToString();
            trackIds[i] = id;
        }

        var tracks = await GetTracksFromIdAsync(clientId, trackIds);
        var orderedTracks = new List<Track>();
        for (int i = 0; i < tracks.Count; i++)
        {
            var origId = trackIds[i];
            for (int j = 0; j < tracks.Count; j++)
            {
                var track = tracks[j];
                var newId = track.ID.ToString();
                if (origId != newId) continue;
                orderedTracks.Add(track);
            }
        }

        var playlist = new Playlist()
        {
            Author = element.GetProperty("user").GetProperty("username").GetString()!,
            ArtworkUrl = element.GetProperty("artwork_url").GetString()!,
            CreatedAt = element.GetProperty("created_at").GetString()!,
            Description = element.GetProperty("description").GetString()!,
            Duration = TimeSpan.FromMilliseconds(element.GetProperty("duration").GetUInt64()),
            Genre = element.GetProperty("genre").GetString()!,
            ID = element.GetProperty("id").GetUInt64(),
            IsAlbum = element.GetProperty("is_album").GetBoolean(),
            LabelName = element.GetProperty("label_name").GetString()!,
            LikesCount = element.GetProperty("likes_count").GetUInt64(),
            PermaLinkUrl = element.GetProperty("permalink_url").GetString()!,
            Public = element.GetProperty("public").GetBoolean(),
            ReleaseDate = element.GetProperty("release_date").GetString()!,
            RepostsCount = element.GetProperty("reposts_count").GetUInt64(),
            SetType = element.GetProperty("set_type").GetString()!,
            TagList = element.GetProperty("tag_list").GetString()!,
            Title = element.GetProperty("title").GetString()!,
            TrackCount = trackCount,
            Tracks = orderedTracks,
            URI = element.GetProperty("uri").GetString()!,
        };
        return playlist;
    }

    internal static ValueTask<Track> CreateTrackAsync(JsonElement info, string clientId)
    {
        var transcodings = info.GetProperty("media").GetProperty("transcodings");
        var trackAuth = info.GetProperty("track_authorization").GetString();
        //TODO: Make an algorithm for choosing the highest quality stream instead of picking the last.
        var baseStreamUrl = transcodings.EnumerateArray().Last().GetProperty("url").GetString();
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

    internal static async Task InsertPlaylistAsync(ICollection<Track> collection, JsonElement info, string clientId)
    {
        var playlist = await CreatePlaylistAsync(info, clientId);
        foreach (var item in playlist.Tracks)
        {
            collection.Add(item);
        }
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

    public static async Task<PlaylistResolveResult> ResolvePlaylistAsync(JsonElement docRoot, string clientId)
    {
        var playlist = await CreatePlaylistAsync(docRoot, clientId);
        return new PlaylistResolveResult() { Playlist = playlist };
    }

    public static async Task<ResolveResult> ResolveAsync(string url, string clientId)
    {
        const string baseUrl = "https://api-v2.soundcloud.com/resolve";
        using var response = await client.GetAsync($"{baseUrl}?url={url}&client_id={clientId}");
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
            ResolveKind.Playlist => await ResolvePlaylistAsync(root, clientId),
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

    public static async Task<IReadOnlyList<Track>> GetTracksFromIdAsync(string clientId, params string[] trackIds)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < trackIds.Length; i++)
        {
            if (i != 0) sb.Append("%2C");
            sb.Append(trackIds[i]);
        }
        var idList = sb.ToString();

        Log.Info($"Getting track with id={idList}...");
        const string baseUrl = "https://api-v2.soundcloud.com/tracks";
        var url = $"{baseUrl}?ids={idList}&client_id={clientId}";

        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Log.Info("Found media!");

        using var doc = await response.Content.ReadAsJsonDocumentAsync();
        var trackElems = doc.RootElement.EnumerateArray();
        var tracks = new List<Track>();
        foreach (var trackElem in trackElems)
        {
            var track = await CreateTrackAsync(trackElem, clientId);
            tracks.Add(track);
        }
        return tracks;
    }

    public static async Task<SearchResult> SearchAsync(string clientId, string query, int searchLimit = 3, ResolveKind? filterKind = null)
    {
        Log.Info($"Searching for {query}...");

        query = query.UrlFriendlyfy();

        const string baseUrl = "https://api-v2.soundcloud.com/search";
        string url = $"{baseUrl}?q={query}&client_id={clientId}&limit={searchLimit}";
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        Log.Info("Found media!");

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
                    if (filterKind == ResolveKind.Playlist) continue;
                    await InsertTrackAsync(tracks, info, clientId);
                    break;
                case "playlist":
                    if (filterKind == ResolveKind.Track) continue;
                    await InsertPlaylistAsync(tracks, info, clientId);
                    break;
                default:
                    continue;
            }
        }
        return new SearchResult { ReturnedMedia = tracks };
    }
}