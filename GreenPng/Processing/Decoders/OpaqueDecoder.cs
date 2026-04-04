using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Decoders;

public sealed class OpaqueDecoder {
    public static void Decode(Span<byte> scanline) {
        int i = 0;

        if(Vector256.IsHardwareAccelerated)
            for(; i < scanline.Length - 31; i += 32)
                Unsafe.As<byte, Vector256<byte>>(ref scanline[i]) |= Vectors256.MaskAlpha;

        i += 3;

        for(; i < scanline.Length; i += 4)
            scanline[i] = 0xFF;
    }
}
