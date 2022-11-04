using FluentAssertions;
using FluentAssertions.Collections;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Utils;

public static class AssertUtils
{
    public static AndConstraint<GenericCollectionAssertions<T>> BeEquivalentToUtf8<T>(
        this GenericCollectionAssertions<T> actualValueShould,
        string expectedValue)
    {
        return actualValueShould.BeEquivalentTo(UTF8.GetBytes(expectedValue));
    }
}
