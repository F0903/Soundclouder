using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;
//TODO: Include all the other info content as well.
public record class Media
{
    internal ClientInfo ClientInfo { get; init; }

    internal string TrackAuth { get; init; } = default!;
    internal string BaseStreamURL { get; init; } = default!;

    public string Title { get; init; } = default!;
    public ulong ID { get; init; } = default!;
    public string Author { get; init; } = default!;
    public string Genre { get; init; } = default!;
}