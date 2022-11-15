using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Monitors
{
// This class is not thread-safe
// Todo: think about concurrency here
    public class ResponsePackageMonitor : IResponsePackageMonitor
    {
        private readonly TimeSpan _exchangeTimeout;
        private readonly IMonitor _monitor;
        private readonly IResponsePackageMonitorStorage _storage;

        public ResponsePackageMonitor(
            TimeSpan exchangeTimeout,
            IMonitor monitor,
            IResponsePackageMonitorStorage storage)
        {
            _exchangeTimeout = exchangeTimeout;
            _monitor = monitor;
            _storage = storage;
        }

        public Package Wait(Guid exchangeId)
        {
            try
            {
                if (_storage.MonitorsObjects.ContainsKey(exchangeId))
                    throw new FatNetLibException($"ExchangeId {exchangeId} is already being waited by someone");
                var responseMonitorObject = new object();
                _storage.MonitorsObjects[exchangeId] = responseMonitorObject;
                WaitingResult waitingResult;
                lock (responseMonitorObject)
                {
                    waitingResult = _monitor.Wait(responseMonitorObject, _exchangeTimeout);
                }

                return waitingResult switch
                {
                    WaitingResult.PulseReceived => _storage.ResponsePackages[exchangeId],
                    WaitingResult.InterruptedByTimeout => throw new FatNetLibException(
                        $"ExchangeId {exchangeId} response timeout exceeded"),
                    _ => throw new FatNetLibException($"{waitingResult} is not supported")
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
            if (responsePackage.ExchangeId == Guid.Empty)
                throw new FatNetLibException("Response package must have an exchangeId");
            Guid exchangeId = responsePackage.ExchangeId;
            bool monitorObjectFound = _storage.MonitorsObjects.Remove(exchangeId, out object? responseMonitorObject);
            if (!monitorObjectFound) return;
            _storage.ResponsePackages[exchangeId] = responsePackage;
            lock (responseMonitorObject!)
            {
                _monitor.Pulse(responseMonitorObject);
            }
        }
    }
}
