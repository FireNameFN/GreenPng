using System;
using BenchmarkDotNet.Attributes;
using GreenPng.Testing;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GreenPng.Benchmarks;

[MemoryDiagnoser(false)]
public class DecodeBenchmarks {
    [ParamsSource(typeof(TestImageDataSource), nameof(TestImageDataSource.GetTestImages))]
    public TestImage TestImage;

    [Benchmark]
    public byte[] DecodeGreenPng() {
        PngDecoder.TryDecodeHeader(TestImage.Png, out PngHeader header);

        byte[] image = new byte[header.ByteSize];

        PngDecoder.TryDecode(TestImage.Png, header, image);

        return image;
    }

    [Benchmark]
    public byte DecodeGreenPngSpan() {
        PngDecoder.TryDecodeHeader(TestImage.Png, out PngHeader header);

        Span<byte> image = stackalloc byte[header.ByteSize];

        PngDecoder.TryDecode(TestImage.Png, header, image);

        return image[^1];
    }

    [Benchmark]
    public byte[] DecodeImageMagick() {
        using MagickImage magick = new(TestImage.Png);

        byte[] image = magick.ToByteArray(MagickFormat.Bgra);

        return image;
    }

    [Benchmark]
    public byte[] DecodeStbImageSharp() {
        byte[] stbImage = StbImageSharp.ImageResult.FromMemory(TestImage.Png, StbImageSharp.ColorComponents.RedGreenBlueAlpha).Data;

        return stbImage;
    }

    [Benchmark]
    public byte[] DecodeImageSharp() {
        using Image<Bgra32> sharp = Image.Load<Bgra32>(TestImage.Png);

        byte[] sharpImage = new byte[sharp.Width * sharp.Height * 4];

        sharp.CopyPixelDataTo(sharpImage);

        return sharpImage;
    }

    [Benchmark]
    public byte DecodeImageSharpSpan() {
        using Image<Bgra32> sharp = Image.Load<Bgra32>(TestImage.Png);

        Span<byte> sharpImage = stackalloc byte[sharp.Width * sharp.Height * 4];

        sharp.CopyPixelDataTo(sharpImage);

        return sharpImage[^1];
    }
}
