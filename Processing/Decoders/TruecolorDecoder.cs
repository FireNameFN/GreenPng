using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Decoders;

public static class TruecolorDecoder {
    public static void Decode(Span<byte> scanline) {
        int i = 0;

        if(Vector256.IsHardwareAccelerated) {
            for(; i < scanline.Length - 31; i += 32) {
                ref Vector256<byte> scanlineVector = ref Unsafe.As<byte, Vector256<byte>>(ref scanline[i]);

                scanlineVector |= Vectors256.MaskAlpha;
            }
        }

        for(; i < scanline.Length; i += 4)
            scanline[i + 3] = 0xFF;
    }
}
