using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using static System.Guid;

namespace Kolyhalov.FatNetLib.Core.Monitors
{
// This class is not thread-safe
// Todo: think about concurrency here
    public class ResponsePackageMonitor : IResponsePackageMonitor
    {
        private readonly Configuration _configuration;

        private readonly IDictionary<Guid, TaskCompletionSource<Package>> _taskCompletionSources =
            new Dictionary<Guid, TaskCompletionSource<Package>>();

        public ResponsePackageMonitor(
            Configuration configuration)
        {
            _configuration = configuration;
        }

        public void WaitAsync(Guid exchangeId, TaskCompletionSource<Package> taskCompletionSource)
        {
            TimeSpan exchangeTimeout = _configuration.ExchangeTimeout!.Value;
            var cancellationToken = new CancellationTokenSource(exchangeTimeout);
            cancellationToken.Token.Register(
                () => taskCompletionSource.TrySetException(new FatNetLibException(
                    $"ExchangeId {exchangeId} response timeout exceeded")),
                useSynchronizationContext: false);

            _taskCompletionSources[exchangeId] = taskCompletionSource;
        }

        public void Pulse(Package responsePackage)
        {
            if (responsePackage.ExchangeId == Empty)
                throw new FatNetLibException($"{nameof(Package.ExchangeId)} is null, which is not allowed");

            Guid exchangeId = responsePackage.ExchangeId!.Value;
            if (_taskCompletionSources.TryGetValue(exchangeId, out TaskCompletionSource<Package> taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(responsePackage);
            }
        }
    }
}
