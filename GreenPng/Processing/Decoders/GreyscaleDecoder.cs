using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Decoders;

public static class GreyscaleDecoder {
    public static void Decode(ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        if(Avx2.IsSupported) {
            DecodeAvx2(encodedScanline, scanline);

            return;
        }

        DecodeScalar(encodedScanline, scanline);
    }

    public static unsafe void DecodeAvx2(ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        byte* encodedScanlinePointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(encodedScanline));

        int i = 0;

        int offset = 0;

        for(; i < encodedScanline.Length - 7; i += 8) {
            Vector256<byte> scanlineVector = Avx2.BroadcastScalarToVector256((ulong*)(encodedScanlinePointer + i)).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.ShuffleMono);

            scanlineVector = Avx2.Or(scanlineVector, Vectors256.MaskAlpha);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DecodeScalar(encodedScanline[i..], scanline[offset..]);
    }

    public static void DecodeScalar(ReadOnlySpan<byte> encodedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < encodedScanline.Length; i++) {
            scanline[offset] = encodedScanline[i];
            scanline[offset + 1] = encodedScanline[i];
            scanline[offset + 2] = encodedScanline[i];
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }
}
