using System;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Attributes
{
    public class FromPackageAttribute : Attribute
    {
        public string Path { get; }

        public FromPackageAttribute(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new FatNetLibException("Path cannot be empty");
            Path = path;
        }
    }
}
