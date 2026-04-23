using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Deserializers;

public static class Deserializer2Bit {
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

        for(; i < serializedScanline.Length - 7; i += 8) {
            Vector256<byte> scanlineVector = Avx2.BroadcastScalarToVector256((ulong*)(serializedScanlinePointer + i)).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle2Bit);

            Vector256<byte> scanlineVectorLow = Avx2.AndNot(scanlineVector, Vectors256.Mask2BitLow);

            Vector256<byte> scanlineVectorHigh = Avx2.AndNot(scanlineVector, Vectors256.Mask2BitHigh);

            scanlineVectorLow = Avx2.And(Avx2.CompareEqual(scanlineVectorLow, default), Vectors256.MaskBit1);

            scanlineVectorHigh = Avx2.And(Avx2.CompareEqual(scanlineVectorHigh, default), Vectors256.MaskBit2);

            scanlineVector = Avx2.Or(scanlineVectorLow, scanlineVectorHigh);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)(b >> 6);
            scanline[offset + 1] = (byte)(b >> 4 & 0b11);
            scanline[offset + 2] = (byte)(b >> 2 & 0b11);
            scanline[offset + 3] = (byte)(b & 0b11);

            offset += 4;
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

        for(; i < serializedScanline.Length - 7; i += 8) {
            Vector256<byte> scanlineVector = Avx2.BroadcastScalarToVector256((ulong*)(serializedScanlinePointer + i)).AsByte();

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle2Bit);

            Vector256<byte> scanlineVectorLow = Avx2.AndNot(scanlineVector, Vectors256.Mask2BitLow);

            Vector256<byte> scanlineVectorHigh = Avx2.AndNot(scanlineVector, Vectors256.Mask2BitHigh);

            scanlineVectorLow = Avx2.CompareEqual(scanlineVectorLow, default);

            scanlineVectorHigh = Avx2.CompareEqual(scanlineVectorHigh, default);

            scanlineVector = Avx2.Xor(scanlineVectorLow, Avx2.And(Vectors256.MaskBit12, Avx2.Xor(scanlineVectorLow, scanlineVectorHigh)));

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScaledScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScaledScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)((b >> 6) * 85);
            scanline[offset + 1] = (byte)((b >> 4 & 0b11) * 85);
            scanline[offset + 2] = (byte)((b >> 2 & 0b11) * 85);
            scanline[offset + 3] = (byte)((b & 0b11) * 85);

            offset += 4;
        }
    }
}
