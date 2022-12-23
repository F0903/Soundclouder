namespace Soundclouder;
internal class MediaStreamCache
{
    readonly Dictionary<ulong, string> cachedMediaStreamUrls = new();
    readonly int ceiling;

    internal MediaStreamCache(int ceiling)
    {
        this.ceiling = ceiling;
    }

    internal void Add(ulong mediaId, string mediaStream)
    {
        if (cachedMediaStreamUrls.Count >= ceiling)
        {
            cachedMediaStreamUrls.Remove(cachedMediaStreamUrls.Keys.First());
        }
        cachedMediaStreamUrls.Add(mediaId, mediaStream);
    }

    internal bool TryGetValue(ulong id, out string? stream) => cachedMediaStreamUrls.TryGetValue(id, out stream);

    internal void Clear() => cachedMediaStreamUrls.Clear();

    internal string this[ulong id] { get => cachedMediaStreamUrls[id]; set => cachedMediaStreamUrls[id] = value; }
}