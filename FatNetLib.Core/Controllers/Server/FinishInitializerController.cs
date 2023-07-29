using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
{
    public class FinishInitializerController : IController
    {
        [Initializer]
        [Route("fat-net-lib/initializers/finish")]
        public async Task<Package> FinishInitializationAsync([Sender] INetPeer clientPeer, ICourier courier)
        {
            await courier.EmitEventAsync(new Package
            {
                Route = InitializationFinished,
                Body = clientPeer
            });
            return new Package();
        }
    }
}
