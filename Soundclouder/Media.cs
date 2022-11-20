using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;
//TODO: Include all the other info content as well.
public record class Media(string TrackAuth, string BaseStreamURL)
{
    internal ClientInfo ClientInfo { get; init; }

    public required string Title { get; init; }
    public required ulong ID { get; init; }
    public required string Author { get; init; }
    public required string Genre { get; init; }
    public required TimeSpan Duration { get; init; }
    public required ulong LikesCount { get; init; }
    public required ulong RepostsCount { get; init; }
    public required ulong CommentCount { get; init; }
    public required string LabelName { get; init;}
    public required string PermaLink { get; init;}
    public required string WaveformUrl { get; init; }
    public required string ArtworkUrl { get; init; }
    public required string ReleaseDate { get; init;}

}