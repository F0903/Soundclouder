namespace Soundclouder.Entities;
//TODO: Include all the other info content as well.
public record Track
{
    public Track(string trackAuth, string baseStreamUrl)
    {
        TrackAuth = trackAuth;
        BaseStreamURL= baseStreamUrl;
    }

    internal string? ClientId { get; init; }
    internal string TrackAuth { get; }
    internal string BaseStreamURL { get; }

    public required string Title { get; init; }
    public required ulong ID { get; init; }
    public required string Author { get; init; }
    public required string Genre { get; init; }
    public required TimeSpan Duration { get; init; }
    public required ulong LikesCount { get; init; }
    public required ulong RepostsCount { get; init; }
    public required ulong CommentCount { get; init; }
    public required string LabelName { get; init; }
    public required string PermaLink { get; init; }
    public required string WaveformUrl { get; init; }
    public required string ArtworkUrl { get; init; }
    public required string ReleaseDate { get; init; }

}