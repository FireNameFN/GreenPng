using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class UpFiltering {
    public static void FilterTruecolor(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        for(; i < filteredScanline.Length - 31; i += 24) {
            Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[offset..]);

            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.Shuffle);

            scanlineVector = (scanlineVector + prevScanlineVector) | Filtering.MaskAlpha;

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[offset] = (byte)(filteredScanline[i + 2] + prevScanline[offset]);
            scanline[offset + 1] = (byte)(filteredScanline[i + 1] + prevScanline[offset + 1]);
            scanline[offset + 2] = (byte)(filteredScanline[i] + prevScanline[offset + 2]);
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }
    public static void FilterTruecolorAlpha(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[i..]);

            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.ShuffleAlpha);

            scanlineVector += prevScanlineVector;

            scanlineVector.CopyTo(scanline[i..]);
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = (byte)(filteredScanline[i + 2] + prevScanline[i]);
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + prevScanline[i + 1]);
            scanline[i + 2] = (byte)(filteredScanline[i] + prevScanline[i + 2]);
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + prevScanline[i + 3]);
        }
    }
}
