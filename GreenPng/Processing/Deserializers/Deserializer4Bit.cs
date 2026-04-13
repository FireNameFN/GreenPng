using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Deserializers;

public static class Deserializer4Bit {
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

        for(; i < serializedScanline.Length - 15; i += 16) {
            Vector256<byte> scanlineVector = Avx2.BroadcastVector128ToVector256(serializedScanlinePointer + i);

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle4Bit);

            Vector256<byte> scanlineVectorHigh = Avx2.ShiftRightLogical(scanlineVector.AsUInt32(), 4).AsByte();

            scanlineVector = Avx2.Xor(scanlineVector, Avx2.And(Vectors256.MaskHalf, Avx2.Xor(scanlineVector, scanlineVectorHigh)));

            scanlineVector = Avx2.And(scanlineVector, Vectors256.MaskHalfBit);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)(b >> 4);
            scanline[offset + 1] = (byte)(b & 0b1111);

            offset += 2;
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

        for(; i < serializedScanline.Length - 15; i += 16) {
            Vector256<byte> scanlineVector = Avx2.BroadcastVector128ToVector256(serializedScanlinePointer + i);

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle4Bit);

            Vector256<byte> scanlineVectorLow = Avx2.ShiftLeftLogical(scanlineVector.AsUInt32(), 4).AsByte();

            Vector256<byte> scanlineVectorHigh = Avx2.ShiftRightLogical(scanlineVector.AsUInt32(), 4).AsByte();

            scanlineVector = Avx2.And(scanlineVector, Vectors256.Mask4Bit);

            scanlineVector = Avx2.Or(scanlineVector, Avx2.And(scanlineVectorLow, Vectors256.Mask4BitLow));

            scanlineVector = Avx2.Or(scanlineVector, Avx2.And(scanlineVectorHigh, Vectors256.Mask4BitHigh));

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        DeserializeScaledScalar(serializedScanline[i..], scanline[offset..]);
    }

    public static void DeserializeScaledScalar(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int offset = 0;

        for(int i = 0; i < serializedScanline.Length; i++) {
            int b = serializedScanline[i];

            scanline[offset] = (byte)(b >> 4 | b & 0b11110000);
            scanline[offset + 1] = (byte)(b & 0b1111 | b << 4);

            offset += 2;
        }
    }
}
