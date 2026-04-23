using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Filters;

public static class SubFiltering {
    public static void Filter(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        if(offset == 4 && Sse2.IsSupported) {
            FilterSse2(filteredScanline, scanline);

            return;
        }

        FilterScalar(filteredScanline, scanline, offset);
    }

    public static void FilterSse2(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        ReadOnlySpan<uint> filteredScanline32 = MemoryMarshal.Cast<byte, uint>(filteredScanline);

        Span<uint> scanline32 = MemoryMarshal.Cast<byte, uint>(scanline);

        Vector128<byte> subScanlineVector = default;

        for(int i = 0; i < scanline32.Length; i++) {
            Vector128<byte> scanlineVector = Sse2.ConvertScalarToVector128UInt32(filteredScanline32[i]).AsByte();

            subScanlineVector = Sse2.Add(scanlineVector, subScanlineVector);

            scanline32[i] = Sse2.ConvertToUInt32(subScanlineVector.AsUInt32());
        }
    }

    public static void FilterScalar(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        filteredScanline[..offset].CopyTo(scanline);

        for(int i = offset; i < scanline.Length; i++)
            scanline[i] = (byte)(filteredScanline[i] + scanline[i - offset]);
    }
}
