using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using GreenPng.Processing.Unpackers;

namespace GreenPng.Processing.Decoders;

public static class IndexedDecoder {
    public static void Decode(ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        Span<byte> lookup = stackalloc byte[1024];

        DecodeLookup(palette, transparency, lookup);

        if(Avx2.IsSupported) {
            DecodeAvx2(lookup, encodedScanline, scanline);

            return;
        }

        DecodeScalar(lookup, encodedScanline, scanline);
    }

    public static unsafe void DecodeAvx2(ReadOnlySpan<byte> lookup, ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        byte* encodedScanlinePointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(encodedScanline));

        uint* lookupPointer = (uint*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(lookup));

        int i = 0;

        int offset = 0;

        for(; i < encodedScanline.Length - 7; i += 8) {
            Vector256<byte> encodedVector = Avx2.BroadcastScalarToVector256((ulong*)(encodedScanlinePointer + i)).AsByte();

            encodedVector = Avx2.Shuffle(encodedVector, Vectors256.ShuffleMono);

            encodedVector = Avx2.And(encodedVector, Vectors256.MaskMono);

            Vector256<byte> scanlineVector = Avx2.GatherVector256(lookupPointer, encodedVector.AsInt32(), 4).AsByte();

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DecodeScalar(lookup, encodedScanline[i..], scanline[offset..]);
    }

    public static void DecodeScalar(ReadOnlySpan<byte> lookup, ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        ReadOnlySpan<uint> lookupPixel = MemoryMarshal.Cast<byte, uint>(lookup);
        Span<uint> scanlinePixel = MemoryMarshal.Cast<byte, uint>(scanline);

        for(int i = 0; i < encodedScanline.Length; i++) {
            byte index = encodedScanline[i];

            uint pixel = lookupPixel[index];

            scanlinePixel[i] = pixel;
        }
    }

    static void DecodeLookup(ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, Span<byte> lookup) {
        if(Avx2.IsSupported) {
            TruecolorUnpacker.UnpackAvx2(palette, lookup);

            DecodeTransparencyAvx2(transparency, lookup);
        } else {
            TruecolorUnpacker.UnpackScalar(palette, lookup);

            DecodeTransparencyScalar(transparency, lookup);
        }

        int transparencyLength = transparency.Length << 2;

        TruecolorDecoder.Decode(lookup[transparencyLength..]);
    }

    static unsafe void DecodeTransparencyAvx2(ReadOnlySpan<byte> transparency, Span<byte> lookup) {
        byte* transparencyPointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(transparency));

        int i = 0;

        int offset = 0;

        for(; i < transparency.Length - 7; i += 8) {
            Vector256<byte> lookupVector = Vector256.Create(lookup[offset..]);

            Vector256<byte> transparencyVector = Avx2.BroadcastScalarToVector256((ulong*)(transparencyPointer + i)).AsByte();

            transparencyVector = Avx2.Shuffle(transparencyVector, Vectors256.ShuffleMono);

            lookupVector = Avx2.Xor(lookupVector, Avx2.And(Vectors256.MaskAlpha, Avx2.Xor(transparencyVector, lookupVector)));

            lookupVector.CopyTo(lookup[offset..]);

            offset += 32;
        }

        DecodeTransparencyScalar(transparency[i..], lookup[offset..]);
    }

    static void DecodeTransparencyScalar(ReadOnlySpan<byte> transparency, Span<byte> lookup) {
        int offset = 3;

        for(int i = 0; i < transparency.Length; i++) {
            lookup[offset] = transparency[i];

            offset += 4;
        }
    }
}
