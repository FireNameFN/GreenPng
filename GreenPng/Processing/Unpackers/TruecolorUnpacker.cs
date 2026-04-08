using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Unpackers;

public static class TruecolorUnpacker {
    public static void Unpack(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        if(Avx2.IsSupported) {
            UnpackAvx2(packedScanline, scanline);

            return;
        }

        UnpackScalar(packedScanline, scanline);
    }

    public static void UnpackAvx2(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        for(; i < packedScanline.Length - 31; i += 24) {
            Vector256<byte> scanlineVector = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(packedScanline[i..]));

            scanlineVector = Avx2.Permute4x64(scanlineVector.AsUInt64(), 0b10_01_01_00).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.ShuffleLane);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        UnpackScalar(packedScanline[i..], scanline[offset..]);
    }

    public static void UnpackScalar(ReadOnlySpan<byte> packedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < packedScanline.Length; i += 3) {
            scanline[offset] = packedScanline[i + 2];
            scanline[offset + 1] = packedScanline[i + 1];
            scanline[offset + 2] = packedScanline[i];

            offset += 4;
        }
    }
}
