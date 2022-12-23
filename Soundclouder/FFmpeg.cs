using Soundclouder.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder;

public class FFmpegNotFoundException : Exception
{
    public FFmpegNotFoundException() : base("Could not find ffmpeg.exe! This is needed to download or convert media. Please make sure the executeable is placed in the same directory as the library.")
    {}
}

internal class FFmpeg
{
    static void AssertFFmpegExists()
    {
        if (!File.Exists("ffmpeg.exe"))
        {
            throw new FFmpegNotFoundException();
        }
    }

    static Process StartProcess(string inputArg, string outputArg)
    {
        AssertFFmpegExists();
        return new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -hide_banner -loglevel error -vn -i {inputArg} {outputArg}",
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true,
            }
        };
    }

    public static async Task DownloadToPath(string path, string url, CancellationToken cancellationToken = default)
    {
        using var proc = StartProcess(url, path);
        Log.Info("Starting FFmpeg...");
        proc.Start();
        Log.Info("Starting conversion...");
        await proc.WaitForExitAsync(cancellationToken);
        var exitCode = proc.ExitCode;
        if (exitCode != 0)
        {
            Log.Error($"FFmpeg exited with error {exitCode}!");
            return;
        }
        Log.Info($"Successfully wrote output to {path}!");
    }
}
