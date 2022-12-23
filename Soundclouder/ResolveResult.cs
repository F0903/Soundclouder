using Soundclouder.Entities;

namespace Soundclouder;

public enum ResolveKind
{
    Track,
    Playlist
}

public abstract record class ResolveResult
{
    public ResolveResult(ResolveKind kind)
    {
        Kind = kind;
    }

    public ResolveKind Kind { get; }
}

public record class TrackResolveResult : ResolveResult
{
    public TrackResolveResult() : base(ResolveKind.Track) { }

    public required Track Track { get; init; }
}

public record class PlaylistResolveResult : ResolveResult
{
    public PlaylistResolveResult() : base(ResolveKind.Playlist) { }


    public required Playlist Playlist { get; init; }
}