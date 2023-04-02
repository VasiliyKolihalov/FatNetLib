using System;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Attributes
{
    public class Field : Attribute
    {
        public string Name { get; }

        public Field(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new FatNetLibException($"{nameof(Name)} cannot be empty");
            Name = name;
        }
    }
}
