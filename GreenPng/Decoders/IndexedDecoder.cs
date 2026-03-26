using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using GreenPng.Filters;

namespace GreenPng.Decoders;

public static class IndexedDecoder {
    static readonly Vector256<byte> Shuffle = Vector256.Create((byte)0, 1, 2, 0, 4, 5, 6, 1, 7, 8, 9, 2, 11, 12, 13, 3, 15, 16, 17, 4, 18, 19, 20, 5, 21, 22, 23, 6, 24, 25, 26, 7);

    static readonly Vector256<byte> Mask = Vector256.Create(0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255);

    public static void Decode(ReadOnlySpan<byte> palette, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        Span<uint> scanlinePixel = MemoryMarshal.Cast<byte, uint>(scanline);

        Span<uint> reversedPalette = stackalloc uint[1024];

        NoneFiltering.Filter(palette, MemoryMarshal.AsBytes(reversedPalette));

        for(int x = 0; x < scanlinePixel.Length; x++) {
            int index = filteredScanline[x];

            uint pixel = reversedPalette[index];

            pixel |= 0xFF000000;

            scanlinePixel[x] = pixel;
        }
    }

    public static void DecodeAlpha(ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        Span<uint> scanlinePixel = MemoryMarshal.Cast<byte, uint>(scanline);

        Span<uint> reversedPalette = stackalloc uint[1024];

        NoneFiltering.Filter(palette, MemoryMarshal.AsBytes(reversedPalette));

        Span<uint> transparencyPalette = stackalloc uint[1024];

        DecodeTransparency(transparency, transparencyPalette);

        for(int x = 0; x < scanlinePixel.Length; x++) {
            int index = filteredScanline[x];

            uint pixel = reversedPalette[index];

            uint alpha = transparencyPalette[index];

            scanlinePixel[x] = pixel & 0xFFFFFF | alpha;
        }
    }

    static void DecodeTransparency(ReadOnlySpan<byte> transparency, Span<uint> transparencyPalette) {
        int i = 0;

        for(; i < transparency.Length - 31; i += 8) {
            Vector256<byte> transparencyVector = Vector256.Create(transparency[i..]);

            Vector256<byte> resultVector = Vector256.ShuffleNative(transparencyVector, Shuffle) & Mask;

            resultVector.AsUInt32().CopyTo(transparencyPalette[i..]);
        }

        for(; i < transparency.Length; i++)
            transparencyPalette[i] = (uint)transparency[i] << 24;
    }
}
