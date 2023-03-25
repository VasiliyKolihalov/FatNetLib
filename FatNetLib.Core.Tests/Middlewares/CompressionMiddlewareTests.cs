using System.IO.Compression;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares;

public class CompressionMiddlewareTests
{
    private const string TestMessage =
        "Вложил десницею кровавой." +
        "И он мне грудь рассек мечом," +
        "И сердце трепетное вынул," +
        "И угль, пылающий огнем," +
        "Во грудь отверстую водвинул." +
        "Как труп в пустыне я лежал," +
        "И бога глас ко мне воззвал:" +
        "\"Востань, пророк, и виждь, и внемли," +
        "Исполнись волею моей" +
        "И, обходя моря и земли," +
        "Глаголом жги сердца людей.\"";

    private static readonly ICompressionAlgorithm Algorithm = new GZipAlgorithm(CompressionLevel.Optimal);
    private readonly CompressionMiddleware _middleware = new(Algorithm);

    [Test]
    public void Process_TestMessage_CompressSerializedField()
    {
        // Arrange
        var package = new Package { Serialized = UTF8.GetBytes(TestMessage) };

        // Act
        _middleware.Process(package);

        // Assert
        string restoredMessage = UTF8.GetString(Algorithm.Decompress(package.Serialized));
        restoredMessage.Should().BeEquivalentTo(TestMessage);
    }
}
