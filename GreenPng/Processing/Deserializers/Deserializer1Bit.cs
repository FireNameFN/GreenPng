using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Deserializers;

public static class Deserializer1Bit {
    public static void Deserialize(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        if(Avx2.IsSupported) {
            DeserializeAvx2(serializedScanline, scanline);

            return;
        }

        DeserializeScalar(serializedScanline, scanline);
    }

    public static unsafe void DeserializeAvx2(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        byte* serializedScanlinePointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(serializedScanline));

        int i = 0;

        int offset = 0;

        for(; i < serializedScanline.Length - 3; i += 4) {
            Vector256<byte> scanlineVector = Avx2.BroadcastScalarToVector256((uint*)(serializedScanlinePointer + i)).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle1Bit);

            scanlineVector = Avx2.AndNot(scanlineVector, Vectors256.Mask1Bit);

            scanlineVector = Avx2.CompareEqual(scanlineVector, default);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)(b >> 7);
            scanline[offset + 1] = (byte)(b >> 6 & 1);
            scanline[offset + 2] = (byte)(b >> 5 & 1);
            scanline[offset + 3] = (byte)(b >> 4 & 1);
            scanline[offset + 4] = (byte)(b >> 3 & 1);
            scanline[offset + 5] = (byte)(b >> 2 & 1);
            scanline[offset + 6] = (byte)(b >> 1 & 1);
            scanline[offset + 7] = (byte)(b & 1);

            offset += 8;
        }
    }

    public static void DeserializeScaled(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        if(Avx2.IsSupported) {
            DeserializeScaledAvx2(serializedScanline, scanline);

            return;
        }

        DeserializeScaledScalar(serializedScanline, scanline);
    }

    public static unsafe void DeserializeScaledAvx2(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        byte* serializedScanlinePointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(serializedScanline));

        int i = 0;

        int offset = 0;

        for(; i < serializedScanline.Length - 3; i += 4) {
            Vector256<byte> scanlineVector = Avx2.BroadcastScalarToVector256((uint*)(serializedScanlinePointer + i)).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle1Bit);

            scanlineVector = Avx2.AndNot(scanlineVector, Vectors256.Mask1Bit);

            scanlineVector = Avx2.CompareEqual(scanlineVector, default);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScaledScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScaledScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)-(b >> 7);
            scanline[offset + 1] = (byte)-(b >> 6 & 1);
            scanline[offset + 2] = (byte)-(b >> 5 & 1);
            scanline[offset + 3] = (byte)-(b >> 4 & 1);
            scanline[offset + 4] = (byte)-(b >> 3 & 1);
            scanline[offset + 5] = (byte)-(b >> 2 & 1);
            scanline[offset + 6] = (byte)-(b >> 1 & 1);
            scanline[offset + 7] = (byte)-(b & 1);

            offset += 8;
        }
    }
}
