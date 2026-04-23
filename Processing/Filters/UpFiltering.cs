using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Filters;

public static class UpFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        if(Vector256.IsHardwareAccelerated) {
            for(; i < scanline.Length - 31; i += 32) {
                Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[i..]);

                Vector256<byte> filteredScanlineVector = Vector256.Create(filteredScanline[i..]);

                Unsafe.As<byte, Vector256<byte>>(ref scanline[i]) = filteredScanlineVector + prevScanlineVector;
            }
        }

        for(; i < scanline.Length; i++)
            scanline[i] = (byte)(filteredScanline[i] + prevScanline[i]);
    }
}
