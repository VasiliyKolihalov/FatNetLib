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
        public Package FinishInitialization([FromPeer] INetPeer clientPeer, ICourier courier)
        {
            courier.EmitEvent(new Package
            {
                Route = InitializationFinished,
                Body = clientPeer
            });
            return new Package();
        }
    }
}
