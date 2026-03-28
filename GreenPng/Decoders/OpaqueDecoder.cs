using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Decoders;

public sealed class OpaqueDecoder {
    public static void Decode(ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = filteredVector | Vectors.MaskAlpha256;

            scanlineVector.CopyTo(scanline[i..]);
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = filteredScanline[i];
            scanline[i + 1] = filteredScanline[i + 1];
            scanline[i + 2] = filteredScanline[i + 2];
            scanline[i + 3] = 0xFF;
        }
    }
}
