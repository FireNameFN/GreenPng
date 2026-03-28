using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Filters;

namespace GreenPng.Benchmarks;

public class FilteringBenchmarks {
    byte[] prevScanline;

    byte[] filtered;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        prevScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        filtered = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = new byte[1024 * 4];
    }

    [Benchmark]
    public void FilterSubVec() {
        SubFiltering.Filter(filtered, scanline);
    }

    [Benchmark]
    public void FilterUpVec() {
        UpFiltering.Filter(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterAverageVec() {
        AverageFiltering.Filter(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterPaethVec() {
        PaethFiltering.Filter(prevScanline, filtered, scanline);
    }
}
