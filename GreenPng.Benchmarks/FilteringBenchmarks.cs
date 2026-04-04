using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Filters;

namespace GreenPng.Benchmarks;

public class FilteringBenchmarks {
    byte[] prevScanline;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        prevScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = RandomNumberGenerator.GetBytes(1024 * 4);
    }

    [Benchmark]
    public void FilterSub() {
        SubFiltering.Filter(scanline);
    }

    [Benchmark]
    public void FilterUp() {
        UpFiltering.Filter(prevScanline, scanline);
    }

    [Benchmark]
    public void FilterAverage() {
        AverageFiltering.Filter(prevScanline, scanline);
    }

    [Benchmark]
    public void FilterPaeth() {
        PaethFiltering.Filter(prevScanline, scanline);
    }
}
