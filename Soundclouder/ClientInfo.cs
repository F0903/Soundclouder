using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;
public record struct ClientInfo
{
    public readonly string ClientId { get; init; }
    public readonly string UserId { get; init; }
}