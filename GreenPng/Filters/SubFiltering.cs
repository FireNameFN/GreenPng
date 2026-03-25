using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class SubFiltering {
    public static void FilterTruecolor(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 24) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.Shuffle);

            scanlineVector += subScanlineVector;

            for(int j = 0; j < 3; j++) {
                Vector256<byte> shift = Filtering.ShiftArray[j];
                Vector256<byte> mask = Filtering.ShiftMaskArray[j];

                scanlineVector += Vector256.ShuffleNative(scanlineVector, shift) & mask;
            }

            scanlineVector |= Filtering.MaskAlpha;

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Filtering.LastShift) & Filtering.LastShiftMask;

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        if(i < 1) {
            scanline[0] = filteredScanline[2];
            scanline[1] = filteredScanline[1];
            scanline[2] = filteredScanline[0];
            scanline[3] = 0xFF;

            i = 3;

            offset = 4;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[offset] = (byte)(filteredScanline[i + 2] + scanline[offset - 4]);
            scanline[offset + 1] = (byte)(filteredScanline[i + 1] + scanline[offset - 3]);
            scanline[offset + 2] = (byte)(filteredScanline[i] + scanline[offset - 2]);
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterTruecolorAlpha(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.ShuffleAlpha);

            scanlineVector += subScanlineVector;

            for(int j = 0; j < 3; j++) {
                Vector256<byte> shift = Filtering.ShiftArray[j];
                Vector256<byte> mask = Filtering.ShiftMaskArray[j];

                scanlineVector += Vector256.ShuffleNative(scanlineVector, shift) & mask;
            }

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Filtering.LastShift) & Filtering.LastShiftMask;

            scanlineVector.CopyTo(scanline[i..]);
        }

        if(i < 1) {
            scanline[0] = filteredScanline[2];
            scanline[1] = filteredScanline[1];
            scanline[2] = filteredScanline[0];
            scanline[3] = filteredScanline[3];
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = (byte)(filteredScanline[i + 2] + scanline[i - 4]);
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + scanline[i - 3]);
            scanline[i + 2] = (byte)(filteredScanline[i] + scanline[i - 2]);
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + scanline[i - 1]);
        }
    }
}
