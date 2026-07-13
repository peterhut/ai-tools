#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0

using System.ComponentModel;
using System.Runtime.InteropServices;

const int DefaultIdleMinutes = 3;
var codexHome = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".codex");
var sessionsPath = Path.Combine(codexHome, "sessions");
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

var idleTimeout = TimeSpan.FromMinutes(DefaultIdleMinutes);
var activity = new ActivityMonitor(idleTimeout);
activity.ObserveExistingFiles(sessionsPath, signalPath);

using var powerRequest = PowerRequest.Create("Codex work or its post-activity sleep countdown is active");
using var watcher = CreateWatcher(codexHome, activity, signalPath);
using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine($"Watching {sessionsPath}");
Console.WriteLine($"Codex becomes idle after {DefaultIdleMinutes} minutes; the Windows 'Sleep after' countdown then begins. Display sleep is unchanged.");
Console.WriteLine("Press Ctrl+C to stop.");

var wasCodexActive = false;
DateTime? releaseAtUtc = null;
while (!cancellation.IsCancellationRequested)
{
    var isCodexActive = activity.IsActive;
    if (isCodexActive && !wasCodexActive)
    {
        releaseAtUtc = null;
        powerRequest.SetSystemRequired(true);
        Console.WriteLine($"[{DateTime.Now:T}] Codex active — preventing system sleep.");
    }
    else if (!isCodexActive && wasCodexActive)
    {
        var sleepAfter = PowerPolicy.GetCurrentSleepAfter();
        if (sleepAfter == TimeSpan.Zero)
        {
            releaseAtUtc = null;
            Console.WriteLine($"[{DateTime.Now:T}] Codex idle — Windows 'Sleep after' is Never; continuing to prevent system sleep.");
        }
        else
        {
            releaseAtUtc = DateTime.UtcNow + sleepAfter;
            Console.WriteLine($"[{DateTime.Now:T}] Codex idle — keeping the system awake for the {FormatDuration(sleepAfter)} Windows 'Sleep after' interval (until {releaseAtUtc.Value.ToLocalTime():T}).");
        }
    }

    if (!isCodexActive && releaseAtUtc is { } releaseAt && DateTime.UtcNow >= releaseAt)
    {
        powerRequest.SetSystemRequired(false);
        releaseAtUtc = null;
        Console.WriteLine($"[{DateTime.Now:T}] Sleep-after interval elapsed — allowing normal system sleep.");
    }

    wasCodexActive = isCodexActive;

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}

static string FormatDuration(TimeSpan duration)
{
    if (duration.TotalMinutes >= 1 && duration.TotalMinutes == Math.Truncate(duration.TotalMinutes))
    {
        return $"{duration.TotalMinutes:0}-minute";
    }

    return $"{duration.TotalSeconds:0}-second";
}

static FileSystemWatcher CreateWatcher(string codexHome, ActivityMonitor activity, string signalPath)
{
    Directory.CreateDirectory(codexHome);
    var watcher = new FileSystemWatcher(codexHome)
    {
        IncludeSubdirectories = true,
        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
        EnableRaisingEvents = true,
    };

    void Observe(object? _, FileSystemEventArgs eventArgs)
    {
        if (IsCodexActivityFile(eventArgs.FullPath, codexHome, signalPath))
        {
            activity.Record();
        }
    }

    watcher.Changed += Observe;
    watcher.Created += Observe;
    watcher.Renamed += (_, eventArgs) =>
    {
        if (IsCodexActivityFile(eventArgs.FullPath, codexHome, signalPath))
        {
            activity.Record();
        }
    };

    watcher.Error += (_, eventArgs) => Console.Error.WriteLine($"File watcher error: {eventArgs.GetException().Message}");
    return watcher;
}

static bool IsCodexActivityFile(string path, string codexHome, string signalPath)
{
    if (string.Equals(path, signalPath, StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    var sessionsPath = Path.Combine(codexHome, "sessions") + Path.DirectorySeparatorChar;
    return path.StartsWith(sessionsPath, StringComparison.OrdinalIgnoreCase)
        && path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase);
}

sealed class ActivityMonitor(TimeSpan idleTimeout)
{
    private DateTime _lastActivityUtc = DateTime.MinValue;

    public bool IsActive => DateTime.UtcNow - _lastActivityUtc < idleTimeout;

    public void Record() => _lastActivityUtc = DateTime.UtcNow;

    public void ObserveExistingFiles(string sessionsPath, string signalPath)
    {
        var newestActivity = File.Exists(signalPath) ? File.GetLastWriteTimeUtc(signalPath) : DateTime.MinValue;

        if (Directory.Exists(sessionsPath))
        {
            foreach (var file in Directory.EnumerateFiles(sessionsPath, "*.jsonl", SearchOption.AllDirectories))
            {
                var lastWriteTimeUtc = File.GetLastWriteTimeUtc(file);
                if (lastWriteTimeUtc > newestActivity)
                {
                    newestActivity = lastWriteTimeUtc;
                }
            }
        }

        if (newestActivity > DateTime.MinValue)
        {
            _lastActivityUtc = newestActivity;
        }
    }
}

sealed class PowerRequest : IDisposable
{
    private const uint PowerRequestContextVersion = 0;
    private readonly SafePowerRequestHandle _handle;
    private bool _isSystemRequired;

    private PowerRequest(SafePowerRequestHandle handle) => _handle = handle;

    public static PowerRequest Create(string reason)
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

        return new PowerRequest(handle);
    }

    public void SetSystemRequired(bool required)
    {
        if (required == _isSystemRequired)
        {
            return;
        }

        if (required)
        {
            if (!NativeMethods.PowerSetRequest(_handle, PowerRequestType.SystemRequired))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set the Windows system power request.");
            }

            _isSystemRequired = true;
            return;
        }

        if (!NativeMethods.PowerClearRequest(_handle, PowerRequestType.SystemRequired))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not clear the Windows system power request.");
        }

        _isSystemRequired = false;
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
