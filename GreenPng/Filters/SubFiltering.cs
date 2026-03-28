using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class SubFiltering {
    public static void Filter(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = filteredVector + subScanlineVector;

            for(int j = 0; j < 3; j++) {
                Vector256<byte> shift = Vectors.ShiftArray256[j];
                Vector256<byte> mask = Vectors.ShiftMaskArray256[j];

                scanlineVector += Vector256.ShuffleNative(scanlineVector, shift) & mask;
            }

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Vectors.LastShift256) & Vectors.LastShiftMask256;

            scanlineVector.CopyTo(scanline[i..]);
        }

        if(i < 1) {
            scanline[0] = filteredScanline[0];
            scanline[1] = filteredScanline[1];
            scanline[2] = filteredScanline[2];
            scanline[3] = filteredScanline[3];

            i = 4;
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = (byte)(filteredScanline[i] + scanline[i - 4]);
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + scanline[i - 3]);
            scanline[i + 2] = (byte)(filteredScanline[i + 2] + scanline[i - 2]);
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + scanline[i - 1]);
        }
    }
}
