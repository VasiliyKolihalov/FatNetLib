using FluentAssertions.Collections;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Utils;

public static class AssertUtils
{
    public static void BeEquivalentToUtf8<T>(this GenericCollectionAssertions<T> actualValueShould,
        string expectedValue)
    {
        actualValueShould.BeEquivalentTo(UTF8.GetBytes(expectedValue));
    }
}