using NAudio.CoreAudioApi;

namespace StableVolume;

/// <summary>
/// Watches the default playback device's master volume and pushes it back to the target
/// whenever something else changes it. Reacts to Windows volume notifications instantly,
/// and a 400 ms safety poll also covers default-device switches.
/// </summary>
public sealed class VolumeGuard : IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly object _gate = new();
    private MMDevice? _device;
    private AudioEndpointVolumeNotificationDelegate? _handler;
    private string _deviceName = "";
    private System.Threading.Timer? _timer;
    private bool _disposed;

    public bool Enabled { get; set; }
    public int TargetPercent { get; set; } = 40;
    public bool CeilingOnly { get; set; }
    public bool KeepUnmuted { get; set; }
    public long Corrections { get; private set; }

    public void Start()
    {
        _timer = new System.Threading.Timer(_ => Tick(), null, 0, 400);
    }

    public string DeviceName
    {
        get { lock (_gate) { return _deviceName; } }
    }

    public int CurrentPercent
    {
        get
        {
            lock (_gate)
            {
                try
                {
                    if (_device == null) return -1;
                    return (int)Math.Round(_device.AudioEndpointVolume.MasterVolumeLevelScalar * 100f);
                }
                catch
                {
                    return -1;
                }
            }
        }
    }

    private void Tick()
    {
        try
        {
            lock (_gate)
            {
                if (_disposed) return;
                Attach();
                if (Enabled) Enforce();
            }
        }
        catch
        {
        }
    }

    private void QueueEnforce()
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                lock (_gate)
                {
                    if (_disposed) return;
                    if (Enabled) Enforce();
                }
            }
            catch
            {
            }
        });
    }

    private void Attach()
    {
        MMDevice? current = null;
        try
        {
            current = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch
        {
            current = null;
        }

        if (current == null)
        {
            Detach();
            _deviceName = "No playback device";
            return;
        }

        if (_device != null && _device.ID == current.ID)
        {
            current.Dispose();
            return;
        }

        Detach();
        _device = current;
        _deviceName = current.FriendlyName;
        _handler = _ => QueueEnforce();
        _device.AudioEndpointVolume.OnVolumeNotification += _handler;
    }

    private void Detach()
    {
        if (_device == null) return;
        try
        {
            if (_handler != null)
            {
                _device.AudioEndpointVolume.OnVolumeNotification -= _handler;
            }
            _device.Dispose();
        }
        catch
        {
        }
        _device = null;
        _handler = null;
    }

    private void Enforce()
    {
        if (_device == null) return;
        var endpoint = _device.AudioEndpointVolume;
        float target = Math.Clamp(TargetPercent, 0, 100) / 100f;
        float now = endpoint.MasterVolumeLevelScalar;
        bool off = CeilingOnly ? now > target + 0.004f : Math.Abs(now - target) > 0.004f;
        if (off)
        {
            endpoint.MasterVolumeLevelScalar = target;
            Corrections++;
        }
        if (KeepUnmuted && endpoint.Mute)
        {
            endpoint.Mute = false;
            Corrections++;
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _timer?.Dispose();
            Detach();
        }
        _enumerator.Dispose();
    }
}
