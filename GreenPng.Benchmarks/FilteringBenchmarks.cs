using System.Runtime.InteropServices;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Filters;

namespace GreenPng.Benchmarks;

public class FilteringBenchmarks {
    byte[] prevScanline;

    uint[] prevScanline4;

    byte[] filtered;

    byte[] filteredAlpha;

    byte[] scanline;

    uint[] scanline4;

    [GlobalSetup]
    public void Setup() {
        prevScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        prevScanline4 = MemoryMarshal.Cast<byte, uint>(RandomNumberGenerator.GetBytes(1024 * 4)).ToArray();

        filtered = RandomNumberGenerator.GetBytes(1024 * 3);

        filteredAlpha = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = new byte[1024 * 4];

        scanline4 = new uint[1024];
    }

    [Benchmark]
    public void FilterNone() {
        Filtering.FilterNone(filtered, scanline);
    }

    [Benchmark]
    public void FilterNoneVec() {
        NoneFiltering.FilterTruecolor(filtered, scanline);
    }

    [Benchmark]
    public void FilterSub() {
        Filtering.FilterSub(filtered, scanline4);
    }

    [Benchmark]
    public void FilterSubVec() {
        SubFiltering.FilterTruecolor(filtered, scanline);
    }

    [Benchmark]
    public void FilterUp() {
        Filtering.FilterUp(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterUpVec() {
        UpFiltering.FilterTruecolor(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterAverage() {
        Filtering.FilterAverage(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterAverageVec() {
        AverageFiltering.FilterTruecolor(prevScanline, filtered, scanline);
    }

    [Benchmark]
    public void FilterPaeth() {
        Filtering.FilterPaeth(prevScanline4, filtered, scanline4);
    }

    [Benchmark]
    public void FilterPaethVec() {
        PaethFiltering.FilterTruecolor(prevScanline, filtered, scanline);
    }
}
