using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Soundclouder.Logging;

public enum LogSeverity
{
    Error,
    Debug,
    Info,
}

public delegate void LogEventHandler(LogSeverity severity, ReadOnlySpan<char> message);

public static class Log
{
    public static event LogEventHandler? Handler;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Out(LogSeverity severity, ReadOnlySpan<char> message)
    {
        Handler?.Invoke(severity, message);
    }

    internal static void Error(ReadOnlySpan<char> message) => Out(LogSeverity.Error, message);
    internal static void Debug(ReadOnlySpan<char> message) => Out(LogSeverity.Debug, message);
    internal static void Info(ReadOnlySpan<char> message) => Out(LogSeverity.Info, message);
}
