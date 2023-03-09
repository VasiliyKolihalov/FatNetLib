using System.IO.Compression;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares;

public class DecompressionMiddlewareTests
{
    private const string TestMessage =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et " +
        "dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut " +
        "aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse " +
        "cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in " +
        "culpa qui officia deserunt mollit anim id est laborum.";

    private static readonly ICompressionAlgorithm Algorithm = new GZipAlgorithm(CompressionLevel.Optimal);
    private readonly DecompressionMiddleware _middleware = new(Algorithm);

    [Test]
    public void Process_TestMessage_DecompressSerializedField()
    {
        // Arrange
        var package = new Package { Serialized = Algorithm.Compress(UTF8.GetBytes(TestMessage)) };

        // Act
        _middleware.Process(package);

        // Assert
        string processedMessage = UTF8.GetString(package.Serialized);
        processedMessage.Should().BeEquivalentTo(TestMessage);
    }
}
