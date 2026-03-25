using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class NoneFiltering {
    public static void FilterTruecolor(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        for(; i < filteredScanline.Length - 31; i += 30) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.Shuffle) | Filtering.MaskAlpha;

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[offset] = filteredScanline[i + 2];
            scanline[offset + 1] = filteredScanline[i + 1];
            scanline[offset + 2] = filteredScanline[i];
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterTruecolorAlpha(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.ShuffleAlpha);

            scanlineVector.CopyTo(scanline[i..]);
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = filteredScanline[i + 2];
            scanline[i + 1] = filteredScanline[i + 1];
            scanline[i + 2] = filteredScanline[i];
            scanline[i + 3] = filteredScanline[i + 3];
        }
    }
}
