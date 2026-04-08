using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Filters;

public static class PaethFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        if(offset == 4 && Ssse3.IsSupported) {
            FilterSsse3(prevScanline, filteredScanline, scanline);

            return;
        }

        FilterScalar(prevScanline, filteredScanline, scanline, offset);
    }

    public static void FilterSsse3(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        ReadOnlySpan<uint> prevScanline32 = MemoryMarshal.Cast<byte, uint>(prevScanline);

        ReadOnlySpan<uint> filteredScanline32 = MemoryMarshal.Cast<byte, uint>(filteredScanline);

        Span<uint> scanline32 = MemoryMarshal.Cast<byte, uint>(scanline);

        Vector128<short> diagonalScanlineVector = default;

        Vector128<short> subScanlineVector = default;

        for(int i = 0; i < scanline32.Length; i++) {
            Vector128<byte> prevScanlineVector = Sse2.ConvertScalarToVector128UInt32(prevScanline32[i]).AsByte();

            Vector128<byte> scanlineVector = Sse2.ConvertScalarToVector128UInt32(filteredScanline32[i]).AsByte();

            Vector128<short> prevScanlineVectorInt16 = Sse2.UnpackLow(prevScanlineVector, default).AsInt16();

            Vector128<byte> scanlineVectorInt16 = Sse2.UnpackLow(scanlineVector, default);

            subScanlineVector = Sse2.Add(scanlineVectorInt16, PaethSsse3(subScanlineVector, prevScanlineVectorInt16, diagonalScanlineVector).AsByte()).AsInt16();

            diagonalScanlineVector = prevScanlineVectorInt16;

            scanline32[i] = Sse2.ConvertToUInt32(Sse2.PackUnsignedSaturate(subScanlineVector, default).AsUInt32());
        }
    }

    public static void FilterScalar(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline, int offset) {
        for(int i = 0; i < offset; i++)
            scanline[i] = (byte)(filteredScanline[i] + Paeth(0, prevScanline[i], 0));

        for(int i = offset; i < scanline.Length; i++)
            scanline[i] = (byte)(filteredScanline[i] + Paeth(scanline[i - offset], prevScanline[i], prevScanline[i - offset]));
    }

    static Vector128<short> PaethSsse3(Vector128<short> a, Vector128<short> b, Vector128<short> c) {
        Vector128<short> pa = Sse2.Subtract(b, c);

        Vector128<short> pb = Sse2.Subtract(a, c);

        Vector128<short> pc = Ssse3.Abs(Sse2.Add(pa, pb)).AsInt16();

        pa = Ssse3.Abs(pa).AsInt16();

        pb = Ssse3.Abs(pb).AsInt16();

        Vector128<short> min = Sse2.Min(pa, Sse2.Min(pb, pc));

        Vector128<short> minb = Sse2.CompareEqual(pb, min);

        Vector128<short> bc = Sse2.Xor(c, Sse2.And(minb, Sse2.Xor(b, c)));

        Vector128<short> mina = Sse2.CompareEqual(pa, min);

        Vector128<short> abc = Sse2.Xor(bc, Sse2.And(mina, Sse2.Xor(a, bc)));

        return abc;
    }

    static byte Paeth(byte a, byte b, byte c) {
        int p = a + b - c;

        int pa = Math.Abs(p - a);

        int pb = Math.Abs(p - b);

        int pc = Math.Abs(p - c);

        if(pa <= pb && pa <= pc)
            return a;

        if(pb <= pc)
            return b;

        return c;
    }
}
