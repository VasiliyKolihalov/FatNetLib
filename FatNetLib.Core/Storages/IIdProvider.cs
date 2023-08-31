using System;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public interface IIdProvider
    {
        Guid GetId(object key);
    }
}
