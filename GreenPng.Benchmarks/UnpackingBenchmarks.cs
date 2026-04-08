using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Unpackers;

namespace GreenPng.Benchmarks;

public class UnpackingBenchmarks {
    byte[] packedScanline;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        packedScanline = RandomNumberGenerator.GetBytes(1024 * 3);

        scanline = RandomNumberGenerator.GetBytes(1024 * 4);
    }

    [Benchmark]
    public void UnpackTruecolor() {
        TruecolorUnpacker.Unpack(packedScanline, scanline);
    }

    [Benchmark]
    public void UnpackTruecolorAvx2() {
        TruecolorUnpacker.UnpackAvx2(packedScanline, scanline);
    }

    [Benchmark]
    public void UnpackTruecolorScalar() {
        TruecolorUnpacker.UnpackScalar(packedScanline, scanline);
    }
}
