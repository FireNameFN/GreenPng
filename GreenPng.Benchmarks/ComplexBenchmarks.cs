using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Decoders;
using GreenPng.Processing.Unpackers;

namespace GreenPng.Benchmarks;

public class ComplexBenchmarks {
    byte[] packedScanline8;

    byte[] packedScanline;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        packedScanline8 = RandomNumberGenerator.GetBytes(1024);

        packedScanline = RandomNumberGenerator.GetBytes(1024 * 3);

        scanline = RandomNumberGenerator.GetBytes(1024 * 4);
    }

    [Benchmark]
    public void DecodePalette() {
        //IndexedDecoder.DecodePalette(packedScanline, scanline);
    }

    [Benchmark]
    public void DecodePalette2() {
        //TruecolorUnpacker.UnpackAvx2(packedScanline, scanline);
        //TruecolorDecoder.DecodeAvx2(scanline);
    }
}
