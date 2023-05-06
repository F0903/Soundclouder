using Soundclouder.Entities;

using System.Diagnostics;

namespace Soundclouder;
public static class MediaExtensions
{
    public static Task<string> GetStreamURLAsync(this Track media) => API.GetStreamURLAsync(media.ClientId ?? throw new NullReferenceException("ClientId was null!"), media);

    public static async Task DownloadAsync(this Track media, string path, CancellationToken cancellationToken = default) =>
        await FFmpeg.DownloadToPath(path, await media.GetStreamURLAsync(), cancellationToken);

    public static IReadOnlyCollection<Track> ToTrackList(this Playlist media) => media.Tracks;

    public static IReadOnlyCollection<Track> ToTrackList(this Track media) => new List<Track> { media };

    public static IReadOnlyCollection<Track> ToTrackList(this SearchResult result) => result.ReturnedMedia;

    public static IReadOnlyCollection<Track> ToTrackList(this ResolveResult result)
    {
        return result switch
        {
            TrackResolveResult tr => tr.Track.ToTrackList(),
            PlaylistResolveResult pr => pr.Playlist.ToTrackList(),
            _ => throw new UnreachableException()
        }; 
    }
}
