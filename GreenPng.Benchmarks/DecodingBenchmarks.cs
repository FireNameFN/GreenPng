using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing;
using GreenPng.Processing.Decoders;

namespace GreenPng.Benchmarks;

public class DecodingBenchmarks {
    byte[] palette;

    byte[] transparency;

    byte[] lookup;

    byte[] encodedScanline8;

    byte[] encodedScanline;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        palette = RandomNumberGenerator.GetBytes(256 * 3);

        transparency = RandomNumberGenerator.GetBytes(256);

        lookup = RandomNumberGenerator.GetBytes(256 * 4);

        encodedScanline8 = RandomNumberGenerator.GetBytes(1024);

        encodedScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = RandomNumberGenerator.GetBytes(1024 * 4);
    }

    /*[Benchmark]
    public void DecodeGreyscale() {
        GreyscaleDecoder.Decode(encodedScanline.AsSpan(0, 1024), scanline);
    }

    [Benchmark]
    public void DecodeGreyscaleVec() {
        Deserializers.Deserialize8(encodedScanline.AsSpan(0, 1024), scanline);
    }

    [Benchmark]
    public void DecodeGreyscaleAvx2() {
        GreyscaleDecoder.DecodeAvx2(encodedScanline.AsSpan(0, 1024), scanline);
    }

    [Benchmark]
    public void DecodeGreyscaleScalar() {
        GreyscaleDecoder.DecodeScalar(encodedScanline.AsSpan(0, 1024), scanline);
    }*/

    [Benchmark]
    public void DecodeGreyscaleAvx2() {
        GreyscaleDecoder.DecodeAvx2(encodedScanline.AsSpan(0, 1024), scanline);
    }

    /*[Benchmark]
    public void DecodeTruecolor() {
        TruecolorDecoder.Decode(scanline);
    }*/

    /*[Benchmark]
    public void DecodeIndexed() {
        IndexedDecoder.Decode(palette, transparency, encodedScanline8, scanline);
    }

    [Benchmark]
    public void DecodeIndexedAvx2() {
        IndexedDecoder.DecodeAvx2(lookup, encodedScanline8, scanline);
    }*/
}
