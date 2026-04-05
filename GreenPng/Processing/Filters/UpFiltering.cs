using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Filters;

public static class UpFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, Span<byte> scanline) {
        int i = 0;

        if(Vector256.IsHardwareAccelerated) {
            for(; i < scanline.Length - 31; i += 32) {
                Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[i..]);

                Unsafe.As<byte, Vector256<byte>>(ref scanline[i]) += prevScanlineVector;
            }
        }

        for(; i < scanline.Length; i++)
            scanline[i] += prevScanline[i];
    }
}
