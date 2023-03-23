using System.IO.Compression;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares;

public class DecompressionMiddlewareTests
{
    private const string TestMessage =
        "Люди приходят с сетями вылавливать картофель из реки, но охрана гонит их прочь; " +
        "они приезжают в дребезжащих автомобилях за выброшенными апельсинами, но керосин уже сделал свое дело. " +
        "И они стоят в оцепенении и смотрят на проплывающий мимо картофель, " +
        "слышат визг свиней, которых режут и засыпают известью в канавах, смотрят на апельсинные горы, " +
        "по которым съезжают вниз оползни зловонной жижи; и в глазах людей поражение; в глазах голодных зреет гнев. " +
        "В душах людей наливаются и зреют гроздья гнева – тяжелые гроздья, и дозревать им теперь уже недолго.";

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
