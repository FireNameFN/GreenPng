using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Filters;

namespace GreenPng.Benchmarks;

public class FilteringBenchmarks {
    byte[] filtered3;

    byte[] filtered4;

    byte[] scanline;

    uint[] scanline4;

    [GlobalSetup]
    public void Setup() {
        filtered3 = RandomNumberGenerator.GetBytes(1024 * 3);

        filtered4 = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = new byte[1024 * 4];

        scanline4 = new uint[1024];
    }

    [Benchmark]
    public void FilterNone() {
        Filtering.FilterNone(filtered3, scanline);
    }

    [Benchmark]
    public void FilterNoneVec() {
        NoneFiltering.FilterTruecolor(filtered3, scanline);
    }

    [Benchmark]
    public void FilterSub() {
        Filtering.FilterSub(filtered3, scanline4);
    }

    [Benchmark]
    public void FilterSubVec() {
        SubFiltering.FilterTruecolor(filtered3, scanline);
    }
}
