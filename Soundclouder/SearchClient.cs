using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;
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

    public Task<SearchResult> SearchAsync(string query, int searchLimit = 3) => API.SearchAsync(clientId, query, searchLimit);

    public Task<ResolveResult> ResolveAsync(string url) => API.ResolveAsync(url, clientId);
}
