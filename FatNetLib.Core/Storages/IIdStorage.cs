using System;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public interface IIdStorage
    {
        Guid GetId(object key);
    }
}
