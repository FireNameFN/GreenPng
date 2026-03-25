using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class AverageFiltering {
    readonly static Vector256<byte> Shift = Vector256.Create((byte)28, 29, 30, 31, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27);

    readonly static Vector256<byte>[] MaskArray;

    static AverageFiltering() {
        MaskArray = new Vector256<byte>[8];

        MaskArray[0] = Vector256.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        for(int i = 0; i < 7; i++)
            MaskArray[i + 1] = Vector256.ShuffleNative(MaskArray[i], Shift);
    }

    public static void FilterTruecolor(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 24) {
            Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[offset..]);

            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.Shuffle);

            for(int j = 0; j < 8; j++) {
                Vector256<byte> subVector = subScanlineVector;

                if(j > 0)
                    subVector = Vector256.ShuffleNative(scanlineVector, Shift);

                Vector256<byte> and = prevScanlineVector & subVector;

                Vector256<byte> xor = (prevScanlineVector ^ subVector) >> 1;

                scanlineVector += (and + xor) & MaskArray[j];
            }

            scanlineVector |= Filtering.MaskAlpha;

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Filtering.LastShift) & Filtering.LastShiftMask;

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        if(i < 1) {
            scanline[0] = (byte)(filteredScanline[2] + scanline[0] / 2);
            scanline[1] = (byte)(filteredScanline[1] + scanline[1] / 2);
            scanline[2] = (byte)(filteredScanline[0] + scanline[2] / 2);
            scanline[3] = 0xFF;

            i = 3;

            offset = 4;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[offset] = (byte)(filteredScanline[i + 2] + (scanline[offset - 4] + prevScanline[offset]) / 2);
            scanline[offset + 1] = (byte)(filteredScanline[i + 1] + (scanline[offset - 3] + prevScanline[offset + 1]) / 2);
            scanline[offset + 2] = (byte)(filteredScanline[i] + (scanline[offset - 2] + prevScanline[offset + 2]) / 2);
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterTruecolorAlpha(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[i..]);

            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.ShuffleAlpha);

            for(int j = 0; j < 8; j++) {
                Vector256<byte> subVector = subScanlineVector;

                if(j > 0)
                    subVector = Vector256.ShuffleNative(scanlineVector, Shift);

                Vector256<byte> and = prevScanlineVector & subVector;

                Vector256<byte> xor = (prevScanlineVector ^ subVector) >> 1;

                scanlineVector += (and + xor) & MaskArray[j];
            }

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Filtering.LastShift) & Filtering.LastShiftMask;

            scanlineVector.CopyTo(scanline[i..]);
        }

        if(i < 1) {
            scanline[0] = (byte)(filteredScanline[2] + scanline[0] / 2);
            scanline[1] = (byte)(filteredScanline[1] + scanline[1] / 2);
            scanline[2] = (byte)(filteredScanline[0] + scanline[2] / 2);
            scanline[3] = 0xFF;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[i] = (byte)(filteredScanline[i + 2] + (scanline[i - 4] + prevScanline[i]) / 2);
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + (scanline[i - 3] + prevScanline[i + 1]) / 2);
            scanline[i + 2] = (byte)(filteredScanline[i] + (scanline[i - 2] + prevScanline[i + 2]) / 2);
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + (scanline[i - 1] + prevScanline[i + 3]) / 2);
        }
    }
}
