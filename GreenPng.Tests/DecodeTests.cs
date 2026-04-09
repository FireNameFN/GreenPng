using System;
using System.Linq;
using System.Threading.Tasks;
using GreenPng.Testing;

namespace GreenPng.Tests;

public sealed class DecodeTests {
    [Test, Skip("Magick.NET is incorrect.")]
    [MethodDataSource(typeof(TestImageDataSource), nameof(TestImageDataSource.GetTestImages))]
    public async Task EqualToImageMagick(TestImage testImage) {
        byte[] image = Decoders.DecodeGreenPng(testImage.Png);

        byte[] imageRef = Decoders.DecodeImageMagick(testImage.Png);

        int compare = 0;

        for(int i = 0; i < image.Length; i++)
            if(image[i] == imageRef[i])
                compare++;
            else
                break;

        await Assert.That(image.SequenceEqual(imageRef)).IsTrue();
    }

    [Test]
    [MethodDataSource(typeof(TestImageDataSource), nameof(TestImageDataSource.GetTestImages))]
    public async Task EqualToStbImageSharp(TestImage testImage) {
        byte[] image = Decoders.DecodeGreenPng(testImage.Png);

        byte[] imageRef = Decoders.DecodeStbImageSharpBgra(testImage.Png);

        await Assert.That(image.SequenceEqual(imageRef)).IsTrue();
    }

    [Test]
    [MethodDataSource(typeof(TestImageDataSource), nameof(TestImageDataSource.GetTestImages))]
    public async Task EqualToImageSharp(TestImage testImage) {
        byte[] image = Decoders.DecodeGreenPng(testImage.Png);

        byte[] imageRef = Decoders.DecodeImageSharp(testImage.Png);

        await Assert.That(image.SequenceEqual(imageRef)).IsTrue();
    }
}
