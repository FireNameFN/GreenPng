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
        SubFiltering.Filter(scanline, 4);
    }

    [Benchmark]
    public void FilterSubSse2() {
        SubFiltering.FilterSse2(scanline);
    }

    [Benchmark]
    public void FilterSubScalar() {
        SubFiltering.FilterScalar(scanline, 4);
    }

    [Benchmark]
    public void FilterUp() {
        UpFiltering.Filter(prevScanline, scanline);
    }

    [Benchmark]
    public void FilterAverage() {
        AverageFiltering.Filter(prevScanline, scanline, 4);
    }

    [Benchmark]
    public void FilterAverageSse2() {
        AverageFiltering.FilterSse2(prevScanline, scanline);
    }

    [Benchmark]
    public void FilterAverageScalar() {
        AverageFiltering.FilterScalar(prevScanline, scanline, 4);
    }

    [Benchmark]
    public void FilterPaeth() {
        PaethFiltering.Filter(prevScanline, scanline, 4);
    }

    [Benchmark]
    public void FilterPaethSsse3() {
        PaethFiltering.FilterSsse3(prevScanline, scanline);
    }

    [Benchmark]
    public void FilterScalar() {
        PaethFiltering.FilterScalar(prevScanline, scanline, 4);
    }
}
