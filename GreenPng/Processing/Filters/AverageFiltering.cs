using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Filters;

public static class AverageFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        if(offset == 4 && Sse2.IsSupported) {
            FilterSse2(prevScanline, filteredScanline, scanline);

            return;
        }

        FilterScalar(prevScanline, filteredScanline, scanline, offset);
    }

    public static void FilterSse2(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        ReadOnlySpan<uint> prevScanline32 = MemoryMarshal.Cast<byte, uint>(prevScanline);

        ReadOnlySpan<uint> filteredScanline32 = MemoryMarshal.Cast<byte, uint>(filteredScanline);

        Span<uint> scanline32 = MemoryMarshal.Cast<byte, uint>(scanline);

        Vector128<byte> subScanlineVector = default;

        for(int i = 0; i < scanline32.Length; i++) {
            Vector128<byte> prevScanlineVector = Sse2.ConvertScalarToVector128UInt32(prevScanline32[i]).AsByte();

            Vector128<byte> scanlineVector = Sse2.ConvertScalarToVector128UInt32(filteredScanline32[i]).AsByte();

            Vector128<byte> average = Sse2.Average(prevScanlineVector, subScanlineVector);

            Vector128<byte> over = Sse2.And(Sse2.Xor(prevScanlineVector, subScanlineVector), Vector128<byte>.One);

            subScanlineVector = Sse2.Add(scanlineVector, Sse2.Subtract(average, over));

            scanline32[i] = Sse2.ConvertToUInt32(subScanlineVector.AsUInt32());
        }
    }

    public static void FilterScalar(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        for(int i = 0; i < offset; i++)
            scanline[i] = (byte)(filteredScanline[i] + prevScanline[i] >> 1);

        for(int i = offset; i < scanline.Length; i++)
            scanline[i] = (byte)(filteredScanline[i] + (scanline[i - offset] + prevScanline[i]) >> 1);
    }
}
