using System;
using System.Runtime.InteropServices;

namespace GreenPng.Filters;

public static class Filtering {
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
