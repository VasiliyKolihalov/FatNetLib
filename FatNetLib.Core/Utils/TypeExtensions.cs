using System;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Utils
{
    public static class TypeExtensions
    {
        public static string ToDependencyId(this Type dependencyType)
        {
            return dependencyType.FullName
                   ?? throw new FatNetLibException($"FullName of {dependencyType} is null");
        }
    }
}
