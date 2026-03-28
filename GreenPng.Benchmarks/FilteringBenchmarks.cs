using System.Runtime.InteropServices;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Filters;

namespace GreenPng.Benchmarks;

public class FilteringBenchmarks {
    byte[] prevScanline;

    uint[] prevScanline4;

    byte[] filtered;

    byte[] scanline;

    uint[] scanline4;

    [GlobalSetup]
    public void Setup() {
        prevScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        prevScanline4 = MemoryMarshal.Cast<byte, uint>(RandomNumberGenerator.GetBytes(1024 * 4)).ToArray();

        filtered = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = new byte[1024 * 4];

        scanline4 = new uint[1024];
    }

    [Benchmark]
    public void FilterSub() {
        Filtering.FilterSub(filtered, scanline4);
    }

    [Benchmark]
    public void FilterSubVec() {
        SubFiltering.Filter(filtered, scanline);
    }

    [Benchmark]
    public void FilterUp() {
        Filtering.FilterUp(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterUpVec() {
        UpFiltering.Filter(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterAverage() {
        Filtering.FilterAverage(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterAverageVec() {
        AverageFiltering.Filter(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterPaeth() {
        Filtering.FilterPaeth(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterPaethVec() {
        PaethFiltering.Filter(prevScanline, filtered, scanline);
    }
}
