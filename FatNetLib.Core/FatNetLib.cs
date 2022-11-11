namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLib
    {
        public IClient Client { get; }

        private readonly INetEventListener _netEventListener;

        public FatNetLib(IClient client, INetEventListener netEventListener)
        {
            Client = client;
            _netEventListener = netEventListener;
        }

        public void Run()
        {
            _netEventListener.Run();
        }

        public void Stop()
        {
            _netEventListener.Stop();
        }
    }
}
