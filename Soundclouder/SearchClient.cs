using Soundclouder.Entities;

using System.Collections;

namespace Soundclouder;

public readonly struct SearchResult : IEnumerable<Track>
{
    public readonly IReadOnlyCollection<Track> ReturnedMedia { get; init; }

    public IEnumerator<Track> GetEnumerator() => ReturnedMedia.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SearchClient
{
    readonly string clientId;

    public SearchClient(string clientId)
    {
        this.clientId = clientId;
    }

    public Task<SearchResult> SearchAsync(string query, int searchLimit = 3, ResolveKind? filterKind = null) => API.SearchAsync(clientId, query, searchLimit, filterKind);

    public Task<ResolveResult> ResolveAsync(string url) => API.ResolveAsync(url, clientId);

    public Task<IReadOnlyList<Track>> GetTracksAsync(params string[] ids) => API.GetTracksFromIdAsync(clientId, ids);
}
