using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;
using System.Collections;

namespace Soundclouder;

public readonly struct SearchResult : IEnumerable<Media>
{
    public readonly IReadOnlyCollection<Media> ReturnedMedia { get; init; }

    public IEnumerator<Media> GetEnumerator() => ReturnedMedia.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SearchClient
{
    readonly ClientInfo clientInfo;

    public SearchClient(ClientInfo clientInfo)
    {
        this.clientInfo = clientInfo;
    }

    public Task<SearchResult> SearchAsync(string query, int searchLimit = 3) => API.SearchAsync(clientInfo, query, searchLimit);
}
