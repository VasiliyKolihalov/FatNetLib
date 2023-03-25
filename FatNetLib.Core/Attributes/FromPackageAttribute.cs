using System;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Attributes
{
    public class FromPackageAttribute : Attribute
    {
        public string Field { get; }

        public FromPackageAttribute(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new FatNetLibException($"{nameof(Field)} cannot be empty");
            Field = field;
        }
    }
}
