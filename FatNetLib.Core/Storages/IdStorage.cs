using System;
using System.Runtime.CompilerServices;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public class IdStorage : IIdStorage
    {
        private readonly ConditionalWeakTable<object, StrongBox<Guid>> _ids =
            new ConditionalWeakTable<object, StrongBox<Guid>>();

        public Guid GetId(object obj)
        {
            return _ids.GetValue(obj, key => new StrongBox<Guid>(Guid.NewGuid())).Value;
        }
    }
}
