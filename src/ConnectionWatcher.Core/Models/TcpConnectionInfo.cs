using System.Net;
using System.Net.NetworkInformation;

namespace ConnectionWatcher.Core.Models;

public sealed record TcpConnectionInfo(
    IPAddress LocalAddress,
    int LocalPort,
    IPAddress RemoteAddress,
    int RemotePort,
    TcpState State,
    int ProcessId,
    string ProcessName,
    string? ProcessPath)
{
    public ConnectionKey Key => new(
        LocalAddress.ToString(),
        LocalPort,
        RemoteAddress.ToString(),
        RemotePort,
        ProcessId);
}

public readonly record struct ConnectionKey(
    string LocalAddress,
    int LocalPort,
    string RemoteAddress,
    int RemotePort,
    int ProcessId);
