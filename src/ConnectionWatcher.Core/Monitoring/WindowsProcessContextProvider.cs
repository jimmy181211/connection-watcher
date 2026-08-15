using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public sealed class WindowsProcessContextProvider : IProcessContextProvider
{
    private const uint Th32csSnapProcess = 0x00000002;
    private const uint ScManagerEnumerateService = 0x0004;
    private const uint ServiceWin32 = 0x00000030;
    private const uint ServiceStateAll = 0x00000003;
    private const int ScEnumProcessInfo = 0;
    private const int ErrorMoreData = 234;
    private static readonly nint InvalidHandleValue = new(-1);

    public ProcessContext GetContext(
        int processId,
        string fallbackName,
        string? fallbackPath)
    {
        Dictionary<int, int> parentIds = ReadParentProcessIds();
        ProcessSnapshot owner = ReadProcess(
            processId,
            fallbackName,
            fallbackPath);
        List<ProcessSnapshot> parents = [];
        HashSet<int> visited = [processId];
        int currentId = processId;
        for (int level = 0; level < 3; level++)
        {
            if (!parentIds.TryGetValue(currentId, out int parentId) ||
                parentId <= 0 ||
                !visited.Add(parentId))
            {
                break;
            }

            ProcessSnapshot parent = ReadProcess(
                parentId,
                $"PID {parentId}",
                null);
            parents.Add(parent);
            currentId = parentId;
        }

        HashSet<int> relatedProcessIds = [processId];
        relatedProcessIds.UnionWith(parents.Select(parent => parent.ProcessId));
        IReadOnlyList<WindowsServiceSnapshot> services =
            ReadServices(relatedProcessIds);
        return new ProcessContext(owner, parents, services);
    }

    private static ProcessSnapshot ReadProcess(
        int processId,
        string fallbackName,
        string? fallbackPath)
    {
        string name = fallbackName;
        string? path = fallbackPath;
        if (processId <= 0)
        {
            name = "System";
        }
        else
        {
            try
            {
                using Process process = Process.GetProcessById(processId);
                name = process.ProcessName;
                try
                {
                    path = process.MainModule?.FileName ?? path;
                }
                catch (Exception ex) when (
                    ex is Win32Exception or InvalidOperationException or NotSupportedException)
                {
                    // The owner is still useful even when Windows denies its path.
                }
            }
            catch (Exception ex) when (
                ex is ArgumentException or InvalidOperationException or Win32Exception)
            {
                // The process may have exited between the network and process snapshots.
            }
        }

        string? product = null;
        string? company = null;
        string? description = null;
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                FileVersionInfo version = FileVersionInfo.GetVersionInfo(path);
                product = EmptyToNull(version.ProductName);
                company = EmptyToNull(version.CompanyName);
                description = EmptyToNull(version.FileDescription);
            }
            catch (Exception ex) when (
                ex is FileNotFoundException or Win32Exception or ArgumentException)
            {
                // File metadata is optional context.
            }
        }

        return new ProcessSnapshot(
            processId,
            name,
            path,
            product,
            company,
            description);
    }

    private static Dictionary<int, int> ReadParentProcessIds()
    {
        Dictionary<int, int> result = [];
        nint snapshot = CreateToolhelp32Snapshot(Th32csSnapProcess, 0);
        if (snapshot == InvalidHandleValue)
        {
            return result;
        }

        try
        {
            ProcessEntry32 entry = new()
            {
                Size = (uint)Marshal.SizeOf<ProcessEntry32>(),
                ExeFile = string.Empty
            };
            if (!Process32First(snapshot, ref entry))
            {
                return result;
            }

            do
            {
                result[(int)entry.ProcessId] = (int)entry.ParentProcessId;
                entry.Size = (uint)Marshal.SizeOf<ProcessEntry32>();
            }
            while (Process32Next(snapshot, ref entry));
        }
        finally
        {
            CloseHandle(snapshot);
        }

        return result;
    }

    private static IReadOnlyList<WindowsServiceSnapshot> ReadServices(
        IReadOnlySet<int> processIds)
    {
        List<WindowsServiceSnapshot> result = [];
        nint manager = OpenSCManager(null, null, ScManagerEnumerateService);
        if (manager == nint.Zero)
        {
            return result;
        }

        try
        {
            uint bytesNeeded = 0;
            uint servicesReturned = 0;
            uint resumeHandle = 0;
            _ = EnumServicesStatusEx(
                manager,
                ScEnumProcessInfo,
                ServiceWin32,
                ServiceStateAll,
                nint.Zero,
                0,
                out bytesNeeded,
                out servicesReturned,
                ref resumeHandle,
                null);
            if (bytesNeeded == 0 && Marshal.GetLastWin32Error() != ErrorMoreData)
            {
                return result;
            }

            nint buffer = Marshal.AllocHGlobal((int)bytesNeeded);
            try
            {
                resumeHandle = 0;
                if (!EnumServicesStatusEx(
                        manager,
                        ScEnumProcessInfo,
                        ServiceWin32,
                        ServiceStateAll,
                        buffer,
                        bytesNeeded,
                        out bytesNeeded,
                        out servicesReturned,
                        ref resumeHandle,
                        null))
                {
                    return result;
                }

                int rowSize = Marshal.SizeOf<EnumServiceStatusProcess>();
                nint row = buffer;
                for (int index = 0; index < servicesReturned; index++)
                {
                    EnumServiceStatusProcess service =
                        Marshal.PtrToStructure<EnumServiceStatusProcess>(row);
                    int serviceProcessId = (int)service.Status.ProcessId;
                    if (processIds.Contains(serviceProcessId))
                    {
                        string serviceName = Marshal.PtrToStringUni(service.ServiceName) ??
                            string.Empty;
                        string displayName = Marshal.PtrToStringUni(service.DisplayName) ??
                            serviceName;
                        result.Add(new WindowsServiceSnapshot(
                            serviceProcessId,
                            serviceName,
                            displayName));
                    }

                    row += rowSize;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            CloseServiceHandle(manager);
        }

        return result
            .OrderBy(service => service.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateToolhelp32Snapshot(uint flags, uint processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool Process32First(nint snapshot, ref ProcessEntry32 entry);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool Process32Next(nint snapshot, ref ProcessEntry32 entry);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint handle);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint OpenSCManager(
        string? machineName,
        string? databaseName,
        uint desiredAccess);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool EnumServicesStatusEx(
        nint serviceControlManager,
        int infoLevel,
        uint serviceType,
        uint serviceState,
        nint services,
        uint bufferSize,
        out uint bytesNeeded,
        out uint servicesReturned,
        ref uint resumeHandle,
        string? groupName);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CloseServiceHandle(nint serviceControlManager);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ProcessEntry32
    {
        public uint Size;
        public uint Usage;
        public uint ProcessId;
        public nuint DefaultHeapId;
        public uint ModuleId;
        public uint Threads;
        public uint ParentProcessId;
        public int BasePriority;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ExeFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct EnumServiceStatusProcess
    {
        public nint ServiceName;
        public nint DisplayName;
        public ServiceStatusProcess Status;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ServiceStatusProcess
    {
        public uint ServiceType;
        public uint CurrentState;
        public uint ControlsAccepted;
        public uint Win32ExitCode;
        public uint ServiceSpecificExitCode;
        public uint CheckPoint;
        public uint WaitHint;
        public uint ProcessId;
        public uint ServiceFlags;
    }
}
