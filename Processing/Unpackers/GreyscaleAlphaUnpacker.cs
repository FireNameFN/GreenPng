using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Unpackers;

public static class GreyscaleAlphaUnpacker {
    public static void Unpack(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        if(Avx2.IsSupported) {
            UnpackAvx2(packedScanline, scanline);

            return;
        }

        UnpackScalar(packedScanline, scanline);
    }

    public static unsafe void UnpackAvx2(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        byte* packedScanlinePointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(packedScanline));

        int i = 0;

        int offset = 0;

        for(; i < packedScanline.Length - 15; i += 16) {
            Vector256<byte> scanlineVector = Avx2.BroadcastVector128ToVector256(packedScanlinePointer + i);

            scanlineVector = Avx2.Shuffle(scanlineVector, Vector256.Create((byte)0, 0, 0, 1, 2, 2, 2, 3, 4, 4, 4, 5, 6, 6, 6, 7, 24, 24, 24, 25, 26, 26, 26, 27, 28, 28, 28, 29, 30, 30, 30, 31));

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        UnpackScalar(packedScanline[i..], scanline[offset..]);
    }

    public static void UnpackScalar(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < packedScanline.Length; i += 2) {
            scanline[offset] = packedScanline[i];
            scanline[offset + 1] = packedScanline[i];
            scanline[offset + 2] = packedScanline[i];
            scanline[offset + 3] = packedScanline[i + 1];

            offset += 4;
        }
    }
}
