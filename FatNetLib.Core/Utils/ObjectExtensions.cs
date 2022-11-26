namespace Kolyhalov.FatNetLib.Core.Utils
{
    public static class ObjectExtensions
    {
        public static bool NotEquals(this object first, object? second)
        {
            return !first.Equals(second);
        }
    }
}
