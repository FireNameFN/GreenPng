using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class Filtering {
    public static readonly Vector256<byte> Shuffle = Vector256.Create((byte)2, 1, 0, 3, 5, 4, 3, 7, 8, 7, 6, 11, 11, 10, 9, 15, 14, 13, 12, 19, 17, 16, 15, 23, 20, 19, 18, 27, 23, 22, 21, 31);

    public static readonly Vector256<byte> ShuffleAlpha = Vector256.Create((byte)2, 1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14, 17, 16, 19, 18, 21, 20, 23, 22, 25, 24, 27, 26, 29, 28, 30);

    public static readonly Vector256<byte> MaskAlpha = Vector256.Create(0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255);

    public static readonly Vector256<byte> MaskColor = Vector256.Create(255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0, 255, 255, 255, 0);

    public static readonly Vector256<byte>[] ShiftArray;

    public static readonly Vector256<byte>[] ShiftMaskArray;

    public static readonly Vector256<byte> LastShift = Vector256.Create((byte)28, 29, 30, 31, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);

    public static readonly Vector256<byte> LastShiftMask = Vector256.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    static Filtering() {
        Span<byte> span = stackalloc byte[44];

        for(int i = 0; i < 28; i++)
            span[i + 16] = (byte)i;

        ShiftArray = new Vector256<byte>[3];

        int offset = 4;

        for(int i = 0; i < 3; i++) {
            ShiftArray[i] = Vector256.Create(span[(16 - offset)..]);

            offset *= 2;
        }

        span[16..].Fill(255);

        ShiftMaskArray = new Vector256<byte>[3];

        offset = 4;

        for(int i = 0; i < 3; i++) {
            ShiftMaskArray[i] = Vector256.Create(span[(16 - offset)..]);

            offset *= 2;
        }
    }

    public static void FilterNone(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < filteredScanline.Length; i += 3) {
            scanline[offset] = filteredScanline[i + 2];
            scanline[offset + 1] = filteredScanline[i + 1];
            scanline[offset + 2] = filteredScanline[i];
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterSub(ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);

        scanlineByte[0] = filteredScanline[2];
        scanlineByte[1] = filteredScanline[1];
        scanlineByte[2] = filteredScanline[0];
        scanlineByte[3] = 0xFF;

        int offset = 4;

        for(int i = 3; i < filteredScanline.Length; i += 3) {
            scanlineByte[offset] = (byte)(filteredScanline[i + 2] + scanlineByte[offset - 4]);
            scanlineByte[offset + 1] = (byte)(filteredScanline[i + 1] + scanlineByte[offset - 3]);
            scanlineByte[offset + 2] = (byte)(filteredScanline[i] + scanlineByte[offset - 2]);
            scanlineByte[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterUp(ReadOnlySpan<uint> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);
        ReadOnlySpan<byte> prevScanlineByte = MemoryMarshal.AsBytes(prevScanline);

        int offset = 0;

        for(int i = 0; i < filteredScanline.Length; i += 3) {
            scanlineByte[offset] = (byte)(filteredScanline[i + 2] + prevScanlineByte[offset]);
            scanlineByte[offset + 1] = (byte)(filteredScanline[i + 1] + prevScanlineByte[offset + 1]);
            scanlineByte[offset + 2] = (byte)(filteredScanline[i] + prevScanlineByte[offset + 2]);
            scanlineByte[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterAverage(ReadOnlySpan<uint> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);
        ReadOnlySpan<byte> prevScanlineByte = MemoryMarshal.AsBytes(prevScanline);

        scanlineByte[0] = (byte)(filteredScanline[2] + prevScanlineByte[0] / 2);
        scanlineByte[1] = (byte)(filteredScanline[1] + prevScanlineByte[1] / 2);
        scanlineByte[2] = (byte)(filteredScanline[0] + prevScanlineByte[2] / 2);
        scanlineByte[3] = 0xFF;

        int offset = 4;

        for(int i = 3; i < filteredScanline.Length; i += 3) {
            scanlineByte[offset] = (byte)(filteredScanline[i + 2] + (scanlineByte[offset - 4] + prevScanlineByte[offset]) / 2);
            scanlineByte[offset + 1] = (byte)(filteredScanline[i + 1] + (scanlineByte[offset - 3] + prevScanlineByte[offset + 1]) / 2);
            scanlineByte[offset + 2] = (byte)(filteredScanline[i] + (scanlineByte[offset - 2] + prevScanlineByte[offset + 2]) / 2);
            scanlineByte[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterPaeth(ReadOnlySpan<uint> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);
        ReadOnlySpan<byte> prevScanlineByte = MemoryMarshal.AsBytes(prevScanline);

        scanlineByte[0] = (byte)(filteredScanline[2] + Paeth(0, prevScanlineByte[0], 0));
        scanlineByte[1] = (byte)(filteredScanline[1] + Paeth(0, prevScanlineByte[1], 0));
        scanlineByte[2] = (byte)(filteredScanline[0] + Paeth(0, prevScanlineByte[2], 0));
        scanlineByte[3] = 0xFF;

        int offset = 4;

        for(int i = 3; i < filteredScanline.Length; i += 3) {
            scanlineByte[offset] = (byte)(filteredScanline[i + 2] + Paeth(scanlineByte[offset - 4], prevScanlineByte[offset], prevScanlineByte[offset - 4]));
            scanlineByte[offset + 1] = (byte)(filteredScanline[i + 1] + Paeth(scanlineByte[offset - 3], prevScanlineByte[offset + 1], prevScanlineByte[offset - 3]));
            scanlineByte[offset + 2] = (byte)(filteredScanline[i] + Paeth(scanlineByte[offset - 2], prevScanlineByte[offset + 2], prevScanlineByte[offset - 2]));
            scanlineByte[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    static byte Paeth(byte a, byte b, byte c) {
        int p = a + b - c;

        int pa = Math.Abs(p - a);

        int pb = Math.Abs(p - b);

        int pc = Math.Abs(p - c);

        if(pa <= pb && pa <= pc)
            return a;

        if(pb <= pc)
            return b;

        return c;
    }
}
