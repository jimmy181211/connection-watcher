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
        TcpState.LastAck,
    ];

    /// <summary>
    /// Test one snapshot against an enabled rule without changing either one.
    /// Listener rules inspect Listen state and the local port only. Remote
    /// rules require an active state, a concrete address and all port filters.
    /// IPv4-mapped IPv6 addresses compare as their IPv4 equivalent.
    /// </summary>
    /// <returns>
    /// True when every applicable condition matches; false for disabled rules,
    /// invalid remote IP filters or ineligible connection states.
    /// </returns>
    public static bool Matches(
        MonitoringRule rule,
        TcpConnectionInfo connection
    )
    {
        if (!rule.Enabled)
        {
            return false;
        }

        if (rule.Type == MonitoringRuleType.LocalListener)
        {
            return connection.State == TcpState.Listen
                && rule.LocalPort.Contains(connection.LocalPort);
        }

        if (
            !ActiveStates.Contains(connection.State)
            || connection.RemoteAddress.Equals(IPAddress.Any)
            || connection.RemoteAddress.Equals(IPAddress.IPv6Any)
        )
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(rule.RemoteIp))
        {
            if (
                !IPAddress.TryParse(rule.RemoteIp, out IPAddress? expected)
                || !Normalize(expected)
                    .Equals(Normalize(connection.RemoteAddress))
            )
            {
                return false;
            }
        }

        return rule.RemotePort.Contains(connection.RemotePort)
            && rule.LocalPort.Contains(connection.LocalPort);
    }

    private static IPAddress Normalize(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }
}
