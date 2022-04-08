using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;
public static class MediaExtensions
{
    public static Task<string> GetStreamURLAsync(this Media media) => API.GetStreamURLAsync(media.ClientInfo, media);

    public static async Task DownloadAsync(this Media media, string path, CancellationToken cancellationToken = default) =>
        await FFmpeg.DownloadToPath(path, await media.GetStreamURLAsync(), cancellationToken);
}
