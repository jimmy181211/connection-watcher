using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public sealed class WindowsTcpConnectionProvider : ITcpConnectionProvider
{
    private const int AfInet = 2;
    private const int AfInet6 = 23;
    private const uint ErrorInsufficientBuffer = 122;

    public IReadOnlyList<TcpConnectionInfo> GetConnections()
    {
        List<TcpConnectionInfo> connections = [];
        Dictionary<int, (string Name, string? Path)> processCache = [];
        ReadIpv4(connections, processCache);
        ReadIpv6(connections, processCache);
        return connections;
    }

    private static void ReadIpv4(
        List<TcpConnectionInfo> destination,
        Dictionary<int, (string Name, string? Path)> processCache)
    {
        ReadTable(AfInet, (buffer, count) =>
        {
            int rowSize = Marshal.SizeOf<MibTcpRowOwnerPid>();
            nint rowPointer = buffer + sizeof(uint);
            for (int index = 0; index < count; index++)
            {
                MibTcpRowOwnerPid row = Marshal.PtrToStructure<MibTcpRowOwnerPid>(rowPointer);
                (string name, string? path) = GetProcessDetails((int)row.OwningPid, processCache);
                destination.Add(new TcpConnectionInfo(
                    new IPAddress(row.LocalAddress),
                    ConvertPort(row.LocalPort),
                    new IPAddress(row.RemoteAddress),
                    ConvertPort(row.RemotePort),
                    (TcpState)row.State,
                    (int)row.OwningPid,
                    name,
                    path));
                rowPointer += rowSize;
            }
        });
    }

    private static void ReadIpv6(
        List<TcpConnectionInfo> destination,
        Dictionary<int, (string Name, string? Path)> processCache)
    {
        ReadTable(AfInet6, (buffer, count) =>
        {
            int rowSize = Marshal.SizeOf<MibTcp6RowOwnerPid>();
            nint rowPointer = buffer + sizeof(uint);
            for (int index = 0; index < count; index++)
            {
                MibTcp6RowOwnerPid row = Marshal.PtrToStructure<MibTcp6RowOwnerPid>(rowPointer);
                (string name, string? path) = GetProcessDetails((int)row.OwningPid, processCache);
                destination.Add(new TcpConnectionInfo(
                    new IPAddress(row.LocalAddress, row.LocalScopeId),
                    ConvertPort(row.LocalPort),
                    new IPAddress(row.RemoteAddress, row.RemoteScopeId),
                    ConvertPort(row.RemotePort),
                    (TcpState)row.State,
                    (int)row.OwningPid,
                    name,
                    path));
                rowPointer += rowSize;
            }
        });
    }

    private static void ReadTable(int addressFamily, Action<nint, int> readRows)
    {
        uint size = 0;
        uint result = GetExtendedTcpTable(
            nint.Zero,
            ref size,
            true,
            addressFamily,
            TcpTableClass.TcpTableOwnerPidAll,
            0);
        if (result != ErrorInsufficientBuffer && result != 0)
        {
            throw new Win32Exception((int)result);
        }

        nint buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            result = GetExtendedTcpTable(
                buffer,
                ref size,
                true,
                addressFamily,
                TcpTableClass.TcpTableOwnerPidAll,
                0);
            if (result != 0)
            {
                throw new Win32Exception((int)result);
            }

            readRows(buffer, Marshal.ReadInt32(buffer));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static (string Name, string? Path) GetProcessDetails(
        int processId,
        Dictionary<int, (string Name, string? Path)> processCache)
    {
        if (processCache.TryGetValue(processId, out (string Name, string? Path) cached))
        {
            return cached;
        }

        (string Name, string? Path) result;
        if (processId <= 0)
        {
            result = ("System", null);
            processCache[processId] = result;
            return result;
        }

        try
        {
            using Process process = Process.GetProcessById(processId);
            string name = process.ProcessName;
            try
            {
                result = (name, process.MainModule?.FileName);
            }
            catch
            {
                result = (name, null);
            }
        }
        catch
        {
            result = ($"PID {processId}", null);
        }

        processCache[processId] = result;
        return result;
    }

    private static int ConvertPort(uint port)
    {
        return (ushort)IPAddress.NetworkToHostOrder((short)(port & 0xffff));
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        nint tcpTable,
        ref uint size,
        [MarshalAs(UnmanagedType.Bool)] bool order,
        int addressFamily,
        TcpTableClass tableClass,
        uint reserved);

    private enum TcpTableClass
    {
        TcpTableBasicListener,
        TcpTableBasicConnections,
        TcpTableBasicAll,
        TcpTableOwnerPidListener,
        TcpTableOwnerPidConnections,
        TcpTableOwnerPidAll
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MibTcpRowOwnerPid
    {
        public uint State;
        public uint LocalAddress;
        public uint LocalPort;
        public uint RemoteAddress;
        public uint RemotePort;
        public uint OwningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MibTcp6RowOwnerPid
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] LocalAddress;
        public uint LocalScopeId;
        public uint LocalPort;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] RemoteAddress;
        public uint RemoteScopeId;
        public uint RemotePort;
        public uint State;
        public uint OwningPid;
    }
}
