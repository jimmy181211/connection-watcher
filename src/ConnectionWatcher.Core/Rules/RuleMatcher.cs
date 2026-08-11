using System.Net;
using System.Net.NetworkInformation;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Rules;

public static class RuleMatcher
{
    private static readonly HashSet<TcpState> ActiveStates =
    [
        TcpState.SynSent,
        TcpState.SynReceived,
        TcpState.Established,
        TcpState.FinWait1,
        TcpState.FinWait2,
        TcpState.CloseWait,
        TcpState.Closing,
        TcpState.LastAck
    ];

    public static bool Matches(MonitoringRule rule, TcpConnectionInfo connection)
    {
        if (!rule.Enabled)
        {
            return false;
        }

        if (rule.Type == MonitoringRuleType.LocalListener)
        {
            return connection.State == TcpState.Listen &&
                rule.LocalPort.Contains(connection.LocalPort);
        }

        if (!ActiveStates.Contains(connection.State) ||
            connection.RemoteAddress.Equals(IPAddress.Any) ||
            connection.RemoteAddress.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(rule.RemoteIp))
        {
            if (!IPAddress.TryParse(rule.RemoteIp, out IPAddress? expected) ||
                !Normalize(expected).Equals(Normalize(connection.RemoteAddress)))
            {
                return false;
            }
        }

        return rule.RemotePort.Contains(connection.RemotePort) &&
            rule.LocalPort.Contains(connection.LocalPort);
    }

    private static IPAddress Normalize(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }
}
