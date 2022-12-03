using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Models;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
{
    public class FinishInitializerController : IController
    {
        [Initializer]
        [Route("fat-net-lib/initializers/finish")]
        public Package FinishInitialization(Package package)
        {
            package.Courier!.EmitEvent(new Package
            {
                Route = InitializationFinished,
                Body = package.FromPeer
            });
            return new Package();
        }
    }
}
