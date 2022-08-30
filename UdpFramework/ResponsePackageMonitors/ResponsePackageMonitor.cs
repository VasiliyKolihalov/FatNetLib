using Kolyhalov.UdpFramework.Configurations;

namespace Kolyhalov.UdpFramework.ResponsePackageMonitors;

// This class is not thread-safe
// todo: think about concurrency here
public class ResponsePackageMonitor : IResponsePackageMonitor
{
    private readonly TimeSpan _exchangeTimeout;
    private readonly IMonitor _monitor;
    private readonly IResponsePackageMonitorStorage _storage;

    public ResponsePackageMonitor(IMonitor monitor, Configuration configuration, IResponsePackageMonitorStorage storage)
    {
        _monitor = monitor;
        _exchangeTimeout = configuration.ExchangeTimeout;
        _storage = storage;
    }

    public Package Wait(Guid exchangeId)
    {
        try
        {
            if (_storage.MonitorsObjects.ContainsKey(exchangeId))
                throw new UdpFrameworkException($"ExchangeId {exchangeId} is already being waited by someone");
            object responseMonitorObject = new();
            _storage.MonitorsObjects[exchangeId] = responseMonitorObject;
            WaitingResult waitingResult;
            lock (responseMonitorObject)
            {
                waitingResult = _monitor.Wait(responseMonitorObject, _exchangeTimeout);
            }

            return waitingResult switch
            {
                WaitingResult.PulseReceived => _storage.ResponsePackages[exchangeId],
                WaitingResult.InterruptedByTimeout => throw new UdpFrameworkException(
                    $"ExchangeId {exchangeId} response timeout exceeded"),
                _ => throw new UdpFrameworkException($"{waitingResult} is not supported")
            };
        }
        finally
        {
            _storage.MonitorsObjects.Remove(exchangeId);
            _storage.ResponsePackages.Remove(exchangeId);
        }
    }

    public void Pulse(Package responsePackage)
    {
        if (responsePackage.ExchangeId == null)
            throw new UdpFrameworkException("Response package must have an exchangeId");
        Guid exchangeId = responsePackage.ExchangeId!.Value;
        bool monitorObjectFound = _storage.MonitorsObjects.Remove(exchangeId, out object? responseMonitorObject);
        if (!monitorObjectFound) return;
        _storage.ResponsePackages[exchangeId] = responsePackage;
        lock (responseMonitorObject!)
        {
            _monitor.Pulse(responseMonitorObject!);
        }
    }
}