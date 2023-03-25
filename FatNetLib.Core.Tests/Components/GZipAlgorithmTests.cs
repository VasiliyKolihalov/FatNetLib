using System.IO.Compression;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class GZipAlgorithmTests
{
    private const string TestMessage =
        "Nel mezzo del cammin di nostra vita" +
        "mi ritrovai per una selva oscura," +
        "ché la diritta via era smarrita." +
        "Ahi quanto a dir qual era è cosa dura" +
        "esta selva selvaggia e aspra e forte" +
        "che nel pensier rinova la paura!" +
        "Tant' è amara che poco è più morte;" +
        "ma per trattar del ben ch'i' vi trovai," +
        "dirò de l'altre cose ch'i' v'ho scorte.";

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
