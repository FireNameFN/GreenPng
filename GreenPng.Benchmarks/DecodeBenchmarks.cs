using System;
using BenchmarkDotNet.Attributes;
using GreenPng.Testing;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GreenPng.Benchmarks;

[MemoryDiagnoser(false)]
public class DecodeBenchmarks {
    readonly byte[] png = Resources.Truecolor;

    [Benchmark]
    public byte[] DecodeIndexedGreenPng() {
        PngDecoder.TryDecodeHeader(png, out PngHeader header);

        byte[] image = new byte[header.ByteSize];

        PngDecoder.TryDecode(png, header, image);

        return image;
    }

    [Benchmark]
    public byte DecodeIndexedGreenPngSpan() {
        PngDecoder.TryDecodeHeader(png, out PngHeader header);

        Span<byte> image = stackalloc byte[header.ByteSize];

        PngDecoder.TryDecode(png, header, image);

        return image[^1];
    }

    [Benchmark]
    public byte[] DecodeIndexedImageMagick() {
        using MagickImage magick = new(png);

        byte[] image = magick.ToByteArray(MagickFormat.Bgra);

        return image;
    }

    [Benchmark]
    public byte[] DecodeIndexedStbImageSharp() {
        byte[] stbImage = StbImageSharp.ImageResult.FromMemory(png, StbImageSharp.ColorComponents.RedGreenBlueAlpha).Data;

        return stbImage;
    }

    [Benchmark]
    public byte[] DecodeIndexedImageSharp() {
        using Image<Bgra32> sharp = Image.Load<Bgra32>(png);

        byte[] sharpImage = new byte[sharp.Width * sharp.Height * 4];

        sharp.CopyPixelDataTo(sharpImage);

        return sharpImage;
    }

    [Benchmark]
    public byte DecodeIndexedImageSharpSpan() {
        using Image<Bgra32> sharp = Image.Load<Bgra32>(png);

        Span<byte> sharpImage = stackalloc byte[sharp.Width * sharp.Height * 4];

        sharp.CopyPixelDataTo(sharpImage);

        return sharpImage[^1];
    }
}
