﻿using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Class | Method)]
public class RouteAttribute : Attribute
{
    public string Route { get; }

    public RouteAttribute(string route)
    {
        Route = route;
    }
}