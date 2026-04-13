using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using GreenPng.Processing.Deserializers;

namespace GreenPng.Benchmarks;

public class DeserializingBenchmarks {
    byte[] serializedScanline1Bit;

    byte[] serializedScanline2Bit;

    byte[] scanline;

    [GlobalSetup]
    public void Setup() {
        serializedScanline1Bit = RandomNumberGenerator.GetBytes(128);

        serializedScanline2Bit = RandomNumberGenerator.GetBytes(256);

        scanline = RandomNumberGenerator.GetBytes(1024);
    }

    [Benchmark]
    public void Deserialize1BitAvx2() {
        Deserializer1Bit.DeserializeAvx2(serializedScanline1Bit, scanline);
    }

    [Benchmark]
    public void Deserialize2BitAvx2() {
        Deserializer2Bit.DeserializeAvx2(serializedScanline2Bit, scanline);
    }
}
