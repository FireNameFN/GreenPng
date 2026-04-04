using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Filters;

public static class AverageFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, Span<byte> scanline) {
        int i = 0;

        if(Vector128.IsHardwareAccelerated) {
            Vector128<byte> subScanlineVector = default;

            for(; i < scanline.Length - 15; i += 16) {
                Vector128<byte> prevScanlineVector = Vector128.Create(prevScanline[i..]);

                Vector128<byte> filteredVector = Vector128.Create(scanline[i..]);

                Vector128<byte> scanlineVector = filteredVector;

                for(int j = 0; j < 4; j++) {
                    Vector128<byte> and = prevScanlineVector & subScanlineVector;

                    Vector128<byte> xor = (prevScanlineVector ^ subScanlineVector) >> 1;

                    scanlineVector += (and + xor) & Vectors128.MaskArray[j];

                    subScanlineVector = Vector128.ShuffleNative(scanlineVector, Vectors128.Shift);
                }

                scanlineVector.CopyTo(scanline[i..]);
            }
        }

        if(i < 1) {
            scanline[0] = (byte)(scanline[0] + scanline[0] / 2);
            scanline[1] = (byte)(scanline[1] + scanline[1] / 2);
            scanline[2] = (byte)(scanline[2] + scanline[2] / 2);
            scanline[3] = (byte)(scanline[3] + scanline[3] / 2);

            i = 4;
        }

        for(; i < scanline.Length; i += 3) {
            scanline[i] = (byte)(scanline[i] + (scanline[i - 4] + prevScanline[i]) / 2);
            scanline[i + 1] = (byte)(scanline[i + 1] + (scanline[i - 3] + prevScanline[i + 1]) / 2);
            scanline[i + 2] = (byte)(scanline[i + 2] + (scanline[i - 2] + prevScanline[i + 2]) / 2);
            scanline[i + 3] = (byte)(scanline[i + 3] + (scanline[i - 1] + prevScanline[i + 3]) / 2);
        }
    }
}
