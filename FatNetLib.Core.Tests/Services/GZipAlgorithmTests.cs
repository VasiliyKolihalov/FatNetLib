using System.IO.Compression;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Services;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests.Services;

public class GZipAlgorithmTests
{
    private const string TestMessage =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et " +
        "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut " +
        "aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse " +
        "cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in " +
        "culpa qui officia deserunt mollit anim id est laborum.";

    private readonly GZipAlgorithm _algorithm = new(CompressionLevel.Optimal);

    [Test]
    public void Compress_TestMessage_ReturnCompressedMessage()
    {
        // Act
        byte[] compressedBytes = _algorithm.Compress(UTF8.GetBytes(TestMessage));

        // Assert
        byte[] decompressedBytes = _algorithm.Decompress(compressedBytes);
        compressedBytes.Length.Should().BeLessThan(decompressedBytes.Length);

        string processedText = UTF8.GetString(decompressedBytes);
        processedText.Should().BeEquivalentTo(TestMessage);
    }
}
