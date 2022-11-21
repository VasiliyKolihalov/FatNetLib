using System;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Core.Attributes
{
    [AttributeUsage(Method)]
    public class InitialAttribute : Attribute
    {
    }
}
