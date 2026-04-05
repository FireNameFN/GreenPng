using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Filters;

public static class AverageFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, Span<byte> scanline, int offset) {
        if(offset == 4 && Sse2.IsSupported) {
            FilterSse2(prevScanline, scanline);

            return;
        }

        FilterScalar(prevScanline, scanline, offset);
    }

    public static void FilterSse2(ReadOnlySpan<byte> prevScanline, Span<byte> scanline) {
        ReadOnlySpan<int> prevScanlineInt32 = MemoryMarshal.Cast<byte, int>(prevScanline);

        Span<int> scanlineInt32 = MemoryMarshal.Cast<byte, int>(scanline);

        Vector128<byte> subScanlineVector = default;

        for(int i = 0; i < scanlineInt32.Length; i++) {
            Vector128<byte> prevScanlineVector = Sse2.ConvertScalarToVector128Int32(prevScanlineInt32[i]).AsByte();

            Vector128<byte> scanlineVector = Sse2.ConvertScalarToVector128Int32(scanlineInt32[i]).AsByte();

            Vector128<byte> average = Sse2.Average(prevScanlineVector, subScanlineVector);

            Vector128<byte> over = Sse2.And(Sse2.Xor(prevScanlineVector, subScanlineVector), Vector128<byte>.One);

            subScanlineVector = Sse2.Add(scanlineVector, Sse2.Subtract(average, over));

            scanlineInt32[i] = Sse2.ConvertToInt32(subScanlineVector.AsInt32());
        }
    }

    public static void FilterScalar(ReadOnlySpan<byte> prevScanline, Span<byte> scanline, int offset) {
        for(int i = 0; i < offset; i++)
            scanline[i] = (byte)(scanline[i] + scanline[i] / 2);

        for(int i = offset; i < scanline.Length; i++)
            scanline[i] = (byte)(scanline[i] + (scanline[i - offset] + prevScanline[i]) / 2);
    }
}
