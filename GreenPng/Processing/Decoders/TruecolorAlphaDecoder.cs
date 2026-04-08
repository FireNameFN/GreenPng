using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Decoders;

public static class TruecolorAlphaDecoder {
    public static void Decode(Span<byte> scanline) {
        if(Avx2.IsSupported) {
            DecodeAvx2(scanline);

            return;
        }

        DecodeScalar(scanline);
    }

    public static void DecodeAvx2(Span<byte> scanline) {
        int i = 0;

        for(; i < scanline.Length - 31; i += 32) {
            Vector256<byte> scanlineVector = Vector256.Create(scanline[i..]);

            scanlineVector = Avx2.Shuffle(scanlineVector, Vectors256.Shuffle);

            scanlineVector.CopyTo(scanline[i..]);
        }

        DecodeScalar(scanline[i..]);
    }

    public static void DecodeScalar(Span<byte> scanline) {
        for(int i = 0; i < scanline.Length; i += 4)
            (scanline[i], scanline[i + 2]) = (scanline[i + 2], scanline[i]);
    }
}
