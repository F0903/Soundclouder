namespace Soundclouder.Entities;

public record Playlist
{
    public required IReadOnlyCollection<Track> Tracks { get; init; }
    public required uint TrackCount { get; init; }
    public required string Title { get; init; }
    public required string Author { get; init; }
    public required string Description { get; init; }
    public required ulong ID { get; init; }
    public required string TagList { get; init; }
    public required string Genre { get; init; }
    public required TimeSpan Duration { get; init; }
    public required ulong LikesCount { get; init; }
    public required ulong RepostsCount { get; init; }
    public required string LabelName { get; init; }
    public required string PermaLinkUrl { get; init; }
    public required string URI { get; init; }
    public required string ArtworkUrl { get; init; }
    public required string CreatedAt { get; init; }
    public required string ReleaseDate { get; init; }
    public required bool Public { get; init; }
    public required bool IsAlbum { get; init; }
    public required string SetType { get; init; }
}
