#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0

using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

var pollInterval = TimeSpan.FromMinutes(1);
var codexHome = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".codex");
var sessionsPath = Path.Combine(codexHome, "sessions");
var threadLocksPath = Path.Combine(codexHome, "thread-writer-locks");
var signalPath = Path.Combine(codexHome, "codex-awake.activity");

if (args.SequenceEqual(["--touch"]))
{
    Directory.CreateDirectory(codexHome);
    File.WriteAllText(signalPath, DateTime.UtcNow.ToString("O"));
    return;
}

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("CodexAwake uses the Windows power-request API and can run only on Windows.");
    return;
}

if (args.SequenceEqual(["--self-test"]))
{
    PowerRequest.AssertSystemRequestCanRecoverAfterWindowsClearsIt();
    Console.WriteLine("PASS: Windows-cleared system power requests can be released and reasserted.");
    return;
}

using var powerRequest = PowerRequest.Create("Codex or OpenCode work, or its post-activity sleep countdown, is active");
using var cancellation = new CancellationTokenSource();
using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(5),
};

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine($"Watching {sessionsPath}");
Console.WriteLine("Polling once per minute; a recent Codex transcript, an active Codex thread lock, or a busy OpenCode session means coding-agent work is active. Display sleep is unchanged.");
Console.WriteLine("Press Ctrl+C to stop.");

var wasAgentActive = false;
var isAgentActive = false;
var wasCodexActive = false;
var isCodexActive = false;
var wasOpenCodeActive = false;
var isOpenCodeActive = false;
var nextPollUtc = DateTime.MinValue;
DateTime? releaseAtUtc = null;
while (!cancellation.IsCancellationRequested)
{
    var nowUtc = DateTime.UtcNow;
    var releaseIsDue = releaseAtUtc is { } releaseAt && nowUtc >= releaseAt;
    var activityWasPolled = false;
    if (nowUtc >= nextPollUtc || releaseIsDue)
    {
        var lastActivityUtc = FindLastActivityUtc(sessionsPath, signalPath);
        var hasActiveThreadWriterLock = HasActiveThreadWriterLock(threadLocksPath);
        var hasRecentTranscriptActivity = nowUtc - lastActivityUtc <= pollInterval;
        isCodexActive = hasActiveThreadWriterLock || hasRecentTranscriptActivity;
        try
        {
            isOpenCodeActive = await OpenCodeActivity.IsActiveAsync(httpClient, cancellation.Token);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            break;
        }

        isAgentActive = isCodexActive || isOpenCodeActive;
        Console.WriteLine(
            $"[{DateTime.Now:T}] Check — Codex: {(isCodexActive ? "active" : "idle")} " +
            $"(writer lock: {(hasActiveThreadWriterLock ? "yes" : "no")}, " +
            $"recent transcript: {(hasRecentTranscriptActivity ? "yes" : "no")}); " +
            $"OpenCode: {(isOpenCodeActive ? "active" : "idle")}");
        nextPollUtc = nowUtc + pollInterval;
        activityWasPolled = true;
    }

    var activeAgentsChanged =
        isCodexActive != wasCodexActive ||
        isOpenCodeActive != wasOpenCodeActive;

    if (isAgentActive && activityWasPolled)
    {
        if (!wasAgentActive)
        {
            releaseAtUtc = null;
        }

        // Windows terminates power requests when the user explicitly sleeps the
        // machine. Renew on every active poll so a watcher that survives sleep
        // cannot retain stale in-process state while the kernel request is gone.
        powerRequest.SetSystemRequired(true);

        if (activeAgentsChanged)
        {
            Console.WriteLine($"[{DateTime.Now:T}] {DescribeActiveAgents(isCodexActive, isOpenCodeActive)} — preventing system sleep.");
        }
    }
    else if (!isAgentActive && wasAgentActive)
    {
        var sleepAfter = PowerPolicy.GetCurrentSleepAfter();
        if (sleepAfter == TimeSpan.Zero)
        {
            releaseAtUtc = null;
            Console.WriteLine($"[{DateTime.Now:T}] Codex and OpenCode idle — Windows 'Sleep after' is Never; continuing to prevent system sleep.");
        }
        else
        {
            releaseAtUtc = DateTime.UtcNow + sleepAfter;
            Console.WriteLine($"[{DateTime.Now:T}] Codex and OpenCode idle — keeping the system awake for the {FormatDuration(sleepAfter)} Windows 'Sleep after' interval (until {releaseAtUtc.Value.ToLocalTime():T}).");
        }
    }

    if (!isAgentActive && releaseAtUtc is { } releaseAtAfterPoll && DateTime.UtcNow >= releaseAtAfterPoll)
    {
        powerRequest.SetSystemRequired(false);
        releaseAtUtc = null;
        Console.WriteLine($"[{DateTime.Now:T}] Sleep-after interval elapsed — allowing normal system sleep.");
    }

    wasAgentActive = isAgentActive;
    wasCodexActive = isCodexActive;
    wasOpenCodeActive = isOpenCodeActive;

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}

static string DescribeActiveAgents(bool isCodexActive, bool isOpenCodeActive) =>
    (isCodexActive, isOpenCodeActive) switch
    {
        (true, true) => "Codex and OpenCode active",
        (true, false) => "Codex active",
        (false, true) => "OpenCode active",
        _ => "No coding agents active",
    };

static string FormatDuration(TimeSpan duration)
{
    if (duration.TotalMinutes >= 1 && duration.TotalMinutes == Math.Truncate(duration.TotalMinutes))
    {
        return $"{duration.TotalMinutes:0}-minute";
    }

    return $"{duration.TotalSeconds:0}-second";
}

static DateTime FindLastActivityUtc(string sessionsPath, string signalPath)
{
    var newestActivityUtc = File.Exists(signalPath)
        ? File.GetLastWriteTimeUtc(signalPath)
        : DateTime.MinValue;

    if (Directory.Exists(sessionsPath))
    {
        foreach (var file in Directory.EnumerateFiles(sessionsPath, "*.jsonl", SearchOption.AllDirectories))
        {
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(file);
            if (lastWriteTimeUtc > newestActivityUtc)
            {
                newestActivityUtc = lastWriteTimeUtc;
            }
        }
    }

    return newestActivityUtc;
}

static bool HasActiveThreadWriterLock(string threadLocksPath)
{
    if (!Directory.Exists(threadLocksPath))
    {
        return false;
    }

    foreach (var lockPath in Directory.EnumerateFiles(threadLocksPath, "*.lock"))
    {
        try
        {
            using var stream = new FileStream(
                lockPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
        }
        catch (FileNotFoundException)
        {
            // The thread finished between enumeration and the probe.
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
        catch (IOException)
        {
            // Codex holds the file open while this thread is active. A sharing
            // violation means the lock is live; stale lock files remain readable.
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // A lock we cannot inspect is not enough evidence of active work.
        }
    }

    return false;
}

static class OpenCodeActivity
{
    private const int AddressFamilyInterNetwork = 2;
    private const int TcpTableOwnerPidListener = 3;

    public static async Task<bool> IsActiveAsync(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var processIds = FindProcessIds("opencode");
        if (processIds.Count == 0)
        {
            return false;
        }

        var ports = FindListeningPorts(processIds);
        if (ports.Count == 0)
        {
            return false;
        }

        var checks = ports.Select(port => IsServerActiveAsync(httpClient, port, cancellationToken));
        var results = await Task.WhenAll(checks);
        return results.Any(result => result);
    }

    private static HashSet<int> FindProcessIds(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        try
        {
            return processes.Select(process => process.Id).ToHashSet();
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static List<int> FindListeningPorts(HashSet<int> processIds)
    {
        var bufferSize = 0;
        NativeMethods.GetExtendedTcpTable(
            IntPtr.Zero,
            ref bufferSize,
            sort: false,
            AddressFamilyInterNetwork,
            TcpTableOwnerPidListener,
            reserved: 0);

        if (bufferSize == 0)
        {
            return [];
        }

        var buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            var result = NativeMethods.GetExtendedTcpTable(
                buffer,
                ref bufferSize,
                sort: false,
                AddressFamilyInterNetwork,
                TcpTableOwnerPidListener,
                reserved: 0);
            if (result != 0)
            {
                return [];
            }

            var ports = new List<int>();
            var rowCount = Marshal.ReadInt32(buffer);
            var rowPointer = IntPtr.Add(buffer, sizeof(uint));
            var rowSize = Marshal.SizeOf<TcpRowOwnerPid>();
            for (var index = 0; index < rowCount; index++)
            {
                var row = Marshal.PtrToStructure<TcpRowOwnerPid>(
                    IntPtr.Add(rowPointer, index * rowSize));
                if (processIds.Contains((int)row.OwningProcessId))
                {
                    ports.Add((ushort)IPAddress.NetworkToHostOrder((short)row.LocalPort));
                }
            }

            return ports;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static async Task<bool> IsServerActiveAsync(
        HttpClient httpClient,
        int port,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(
                $"http://127.0.0.1:{port}/session/status",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                content,
                cancellationToken: cancellationToken);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            return document.RootElement
                .EnumerateObject()
                .Any(session =>
                    !session.Value.TryGetProperty("type", out var type) ||
                    !string.Equals(type.GetString(), "idle", StringComparison.OrdinalIgnoreCase));
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TcpRowOwnerPid
    {
        public uint State;
        public uint LocalAddress;
        public uint LocalPort;
        public uint RemoteAddress;
        public uint RemotePort;
        public uint OwningProcessId;
    }

    private static class NativeMethods
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedTcpTable(
            IntPtr tcpTable,
            ref int outputBufferLength,
            [MarshalAs(UnmanagedType.Bool)] bool sort,
            int addressFamily,
            int tableClass,
            uint reserved);
    }
}

sealed class PowerRequest : IDisposable
{
    private const uint PowerRequestContextVersion = 0;
    private readonly string _reason;
    private SafePowerRequestHandle _handle;
    private bool _isSystemRequired;

    private PowerRequest(string reason, SafePowerRequestHandle handle)
    {
        _reason = reason;
        _handle = handle;
    }

    public static PowerRequest Create(string reason)
        => new(reason, CreateHandle(reason));

    private static SafePowerRequestHandle CreateHandle(string reason)
    {
        var context = new ReasonContext
        {
            Version = PowerRequestContextVersion,
            Flags = 0x1, // POWER_REQUEST_CONTEXT_SIMPLE_STRING
            SimpleReasonString = reason,
        };

        var handle = NativeMethods.PowerCreateRequest(ref context);
        if (handle.IsInvalid)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create the Windows power request.");
        }

        return handle;
    }

    public void SetSystemRequired(bool required)
    {
        if (!required && !_isSystemRequired)
        {
            return;
        }

        if (required)
        {
            if (_isSystemRequired)
            {
                var replacementHandle = CreateHandle(_reason);
                if (!NativeMethods.PowerSetRequest(replacementHandle, PowerRequestType.SystemRequired))
                {
                    var error = Marshal.GetLastWin32Error();
                    replacementHandle.Dispose();
                    throw new Win32Exception(error, "Could not renew the Windows system power request.");
                }

                var previousHandle = _handle;
                _handle = replacementHandle;

                // This can legitimately fail if Windows already terminated the
                // old request during user-initiated sleep. Closing the old handle
                // still guarantees its request object is cleaned up.
                NativeMethods.PowerClearRequest(previousHandle, PowerRequestType.SystemRequired);
                previousHandle.Dispose();
                return;
            }

            if (!NativeMethods.PowerSetRequest(_handle, PowerRequestType.SystemRequired))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set the Windows system power request.");
            }

            _isSystemRequired = true;
            return;
        }

        var inactiveHandle = CreateHandle(_reason);
        var activeHandle = _handle;
        _handle = inactiveHandle;
        _isSystemRequired = false;

        // Closing the request object guarantees cleanup. PowerClearRequest can
        // legitimately fail here if Windows already terminated the request.
        NativeMethods.PowerClearRequest(activeHandle, PowerRequestType.SystemRequired);
        activeHandle.Dispose();
    }

    public static void AssertSystemRequestCanRecoverAfterWindowsClearsIt()
    {
        using var request = Create("CodexAwake power-request self-test");
        request.SetSystemRequired(true);

        // User-initiated sleep clears the kernel request without changing this
        // process's bookkeeping. Reproduce that state without sleeping the PC.
        if (!NativeMethods.PowerClearRequest(request._handle, PowerRequestType.SystemRequired))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not simulate Windows clearing the power request.");
        }

        request.SetSystemRequired(false);
        request.SetSystemRequired(true);
        if (!NativeMethods.PowerClearRequest(request._handle, PowerRequestType.SystemRequired))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not simulate Windows clearing the renewed power request.");
        }

        request.SetSystemRequired(true);
        if (!NativeMethods.PowerClearRequest(request._handle, PowerRequestType.SystemRequired))
        {
            throw new InvalidOperationException("The power request remained cleared after it was reasserted.");
        }

        request._isSystemRequired = false;
    }

    public void Dispose()
    {
        if (_isSystemRequired)
        {
            NativeMethods.PowerClearRequest(_handle, PowerRequestType.SystemRequired);
        }

        _handle.Dispose();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ReasonContext
    {
        public uint Version;
        public uint Flags;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string SimpleReasonString;
    }

    private sealed class SafePowerRequestHandle : SafeHandle
    {
        public SafePowerRequestHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero || handle == new IntPtr(-1);

        protected override bool ReleaseHandle() => NativeMethods.CloseHandle(handle);
    }

    private enum PowerRequestType : uint
    {
        DisplayRequired = 0,
        SystemRequired = 1,
        AwayModeRequired = 2,
        ExecutionRequired = 3,
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern SafePowerRequestHandle PowerCreateRequest(ref ReasonContext context);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PowerSetRequest(SafePowerRequestHandle handle, PowerRequestType requestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PowerClearRequest(SafePowerRequestHandle handle, PowerRequestType requestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);
    }
}

static class PowerPolicy
{
    private static readonly Guid SleepSubgroup = new("238c9fa8-0aad-41ed-83f4-97be242c8f20");
    private static readonly Guid SleepAfterSetting = new("29f6c1db-86da-48c5-9fdb-f2b67b1f44da");

    public static TimeSpan GetCurrentSleepAfter()
    {
        var result = NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out var schemePointer);
        if (result != 0)
        {
            throw new Win32Exception((int)result, "Could not read the active Windows power scheme.");
        }

        try
        {
            var scheme = Marshal.PtrToStructure<Guid>(schemePointer);
            if (!NativeMethods.GetSystemPowerStatus(out var status))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not determine whether Windows is using AC or battery power.");
            }

            var sleepSubgroup = SleepSubgroup;
            var sleepAfterSetting = SleepAfterSetting;
            uint seconds;
            result = status.AcLineStatus == 0
                ? NativeMethods.PowerReadDcValueIndex(IntPtr.Zero, ref scheme, ref sleepSubgroup, ref sleepAfterSetting, out seconds)
                : NativeMethods.PowerReadAcValueIndex(IntPtr.Zero, ref scheme, ref sleepSubgroup, ref sleepAfterSetting, out seconds);

            if (result != 0)
            {
                throw new Win32Exception((int)result, "Could not read the Windows 'Sleep after' setting.");
            }

            return TimeSpan.FromSeconds(seconds);
        }
        finally
        {
            NativeMethods.LocalFree(schemePointer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte AcLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte SystemStatusFlag;
        public uint BatteryLifeTime;
        public uint BatteryFullLifeTime;
    }

    private static class NativeMethods
    {
        [DllImport("powrprof.dll")]
        internal static extern uint PowerGetActiveScheme(IntPtr userRootPowerKey, out IntPtr activePolicyGuid);

        [DllImport("powrprof.dll", EntryPoint = "PowerReadACValueIndex")]
        internal static extern uint PowerReadAcValueIndex(
            IntPtr rootPowerKey,
            ref Guid schemeGuid,
            ref Guid subgroupGuid,
            ref Guid powerSettingGuid,
            out uint valueIndex);

        [DllImport("powrprof.dll", EntryPoint = "PowerReadDCValueIndex")]
        internal static extern uint PowerReadDcValueIndex(
            IntPtr rootPowerKey,
            ref Guid schemeGuid,
            ref Guid subgroupGuid,
            ref Guid powerSettingGuid,
            out uint valueIndex);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetSystemPowerStatus(out SystemPowerStatus systemPowerStatus);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LocalFree(IntPtr memory);
    }
}
