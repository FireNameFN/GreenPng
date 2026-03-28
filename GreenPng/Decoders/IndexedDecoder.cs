using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Decoders;

public static class IndexedDecoder {
    public static unsafe void Decode(ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        Span<uint> scanlinePixel = MemoryMarshal.Cast<byte, uint>(scanline);

        Span<byte> lookup = stackalloc byte[1024];

        Deserializer.Deserialize24(palette, lookup);

        if(transparency.Length > 0)
            DecodeTransparency(transparency, lookup);
        else
            TruecolorDecoder.Decode(lookup, lookup);

        uint* lookupPointer = (uint*)Unsafe.AsPointer(ref lookup[0]);

        int i = 0;

        for(; i < filteredScanline.Length - 31; i += 8) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> indexVector = filteredVector & Vectors.MaskMono256;

            Vector256<uint> scanlineVector = Avx2.GatherVector256(lookupPointer, indexVector.AsInt32(), 4);

            scanlineVector.AsByte().CopyTo(scanline[i..]);
        }

        for(; i < filteredScanline.Length; i += 4) {
            int index = filteredScanline[i];

            uint pixel = lookupPointer[index];

            Unsafe.As<byte, uint>(ref scanline[i]) = pixel;
        }
    }

    static void DecodeTransparency(ReadOnlySpan<byte> transparency, Span<byte> lookup) {
        int i = 0;

        int offset = 0;

        for(; i < transparency.Length - 31; i += 8) {
            Vector256<byte> transparencyVector = Vector256.Create(transparency[i..]);

            Vector256<byte> lookupVector = Vector256.Create(lookup[offset..]);

            transparencyVector = Vector256.ShuffleNative(transparencyVector, Vectors.ShuffleMono256);

            lookupVector = Vector256.ConditionalSelect(Vectors.MaskAlpha256, transparencyVector, lookupVector);

            lookupVector.CopyTo(lookup[offset..]);

            offset += 32;
        }

        offset += 3;

        for(; i < transparency.Length; i++) {
            lookup[offset] = transparency[i];

            offset += 4;
        }
    }
}
