using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Filters;

namespace GreenPng.Benchmarks;

[MemoryDiagnoser(false)]
public class FilteringBenchmarks {
    byte[] prevScanline;

    byte[] filteredScanline;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        prevScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        filteredScanline = RandomNumberGenerator.GetBytes(1024 * 4);

        scanline = RandomNumberGenerator.GetBytes(1024 * 4);
    }

    [Benchmark]
    public void FilterSub() {
        SubFiltering.Filter(filteredScanline, scanline, 4);
    }

    /*[Benchmark]
    public void FilterSubSse2() {
        SubFiltering.FilterSse2(filteredScanline, scanline);
    }

    [Benchmark]
    public void FilterSubScalar() {
        SubFiltering.FilterScalar(filteredScanline, scanline, 4);
    }*/

    [Benchmark]
    public void FilterSubMono() {
        SubFiltering.Filter(filteredScanline.AsSpan(0, 1024), scanline.AsSpan(0, 1024), 1);
    }

    /*[Benchmark]
    public void FilterUp() {
        UpFiltering.Filter(prevScanline, filteredScanline, scanline);
    }

    [Benchmark]
    public void FilterUpMono() {
        UpFiltering.Filter(prevScanline.AsSpan(0, 1024), filteredScanline.AsSpan(0, 1024), scanline.AsSpan(0, 1024));
    }

    [Benchmark]
    public void FilterAverage() {
        AverageFiltering.Filter(prevScanline, filteredScanline, scanline, 4);
    }*/

    /*[Benchmark]
    public void FilterAverageSse2() {
        AverageFiltering.FilterSse2(prevScanline, filteredScanline, scanline);
    }

    [Benchmark]
    public void FilterAverageScalar() {
        AverageFiltering.FilterScalar(prevScanline, filteredScanline, scanline, 4);
    }*/

    /*[Benchmark]
    public void FilterAverageMono() {
        AverageFiltering.Filter(prevScanline.AsSpan(0, 1024), filteredScanline.AsSpan(0, 1024), scanline.AsSpan(0, 1024), 1);
    }*/

    [Benchmark]
    public void FilterPaeth() {
        PaethFiltering.Filter(prevScanline, filteredScanline, scanline, 4);
    }

    /*[Benchmark]
    public void FilterPaethSsse3() {
        PaethFiltering.FilterSsse3(prevScanline, filteredScanline, scanline);
    }

    [Benchmark]
    public void FilterPaethScalar() {
        PaethFiltering.FilterScalar(prevScanline, filteredScanline, scanline, 4);
    }*/

    [Benchmark]
    public void FilterPaethMono() {
        PaethFiltering.Filter(prevScanline.AsSpan(0, 1024), filteredScanline.AsSpan(0, 1024), scanline.AsSpan(0, 1024), 1);
    }
}
