using System;
using System.Runtime.Intrinsics;

namespace GreenPng;

public static class Deserializer {
    public static void Deserialize8(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        for(; i < serializedScanline.Length - 31; i += 8) {
            Vector256<byte> serializedVector = Vector256.Create(serializedScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(serializedVector, Vectors.ShuffleMono256);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        for(; i < serializedScanline.Length; i++) {
            scanline[offset] = serializedScanline[i];
            scanline[offset + 1] = serializedScanline[i];
            scanline[offset + 2] = serializedScanline[i];
            scanline[offset + 3] = serializedScanline[i];

            offset += 4;
        }
    }

    public static void Deserialize24(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        for(; i < serializedScanline.Length - 31; i += 24) {
            Vector256<byte> serializedVector = Vector256.Create(serializedScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(serializedVector, Vectors.Shuffle256);

            scanlineVector.CopyTo(scanline[offset..]);

            offset += 32;
        }

        for(; i < serializedScanline.Length; i += 3) {
            scanline[offset] = serializedScanline[i + 2];
            scanline[offset + 1] = serializedScanline[i + 1];
            scanline[offset + 2] = serializedScanline[i];

            offset += 4;
        }
    }

    public static void Deserialize32(ReadOnlySpan<byte> serializedScanline, Span<byte> scanline) {
        int i = 0;

        for(; i < serializedScanline.Length - 31; i += 32) {
            Vector256<byte> serializedVector = Vector256.Create(serializedScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(serializedVector, Vectors.ShuffleAlpha256);

            scanlineVector.CopyTo(scanline[i..]);
        }

        for(; i < serializedScanline.Length; i += 4) {
            scanline[i] = serializedScanline[i + 2];
            scanline[i + 1] = serializedScanline[i + 1];
            scanline[i + 2] = serializedScanline[i];
            scanline[i + 3] = serializedScanline[i + 3];
        }
    }
}
