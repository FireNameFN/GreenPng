using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Processing.Filters;

public static class PaethFiltering {
    public static void Filter(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        if(Vector256.IsHardwareAccelerated && Vector128.IsHardwareAccelerated) {
            Vector128<byte> diagonalScanlineVector = default;

            Vector128<byte> subScanlineVector = default;

            for(; i < filteredScanline.Length - 15; i += 4) {
                Vector128<byte> prevScanlineVector = Vector128.Create(prevScanline[i..]);

                Vector128<byte> filteredVector = Vector128.Create(filteredScanline[i..]);

                Vector128<byte> scanlineVector = filteredVector;

                scanlineVector += Paeth(subScanlineVector, prevScanlineVector, diagonalScanlineVector) & Vectors128.PixelMask;

                scanlineVector.CopyTo(scanline[i..]);

                diagonalScanlineVector = prevScanlineVector;

                subScanlineVector = scanlineVector;
            }
        }

        if(i < 1) {
            scanline[0] = (byte)(filteredScanline[0] + Paeth(0, scanline[0], 0));
            scanline[1] = (byte)(filteredScanline[1] + Paeth(0, scanline[1], 0));
            scanline[2] = (byte)(filteredScanline[2] + Paeth(0, scanline[2], 0));
            scanline[3] = (byte)(filteredScanline[3] + Paeth(0, scanline[3], 0));

            i = 4;
        }

        for(; i < filteredScanline.Length; i += 4) {
            scanline[i] = (byte)(filteredScanline[i] + Paeth(scanline[i - 4], prevScanline[i], prevScanline[i - 4]));
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + Paeth(scanline[i - 3], prevScanline[i + 1], prevScanline[i - 3]));
            scanline[i + 2] = (byte)(filteredScanline[i + 2] + Paeth(scanline[i - 2], prevScanline[i + 2], prevScanline[i - 2]));
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + Paeth(scanline[i - 1], prevScanline[i + 3], prevScanline[i - 1]));
        }
    }

    static Vector128<byte> Paeth(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c) {
        (Vector128<ushort> a1, Vector128<ushort> a2) = Vector128.Widen(a);
        (Vector128<ushort> b1, Vector128<ushort> b2) = Vector128.Widen(b);
        (Vector128<ushort> c1, Vector128<ushort> c2) = Vector128.Widen(c);

        Vector256<short> sa = Vector256.Create(a1.AsInt16(), a2.AsInt16());
        Vector256<short> sb = Vector256.Create(b1.AsInt16(), b2.AsInt16());
        Vector256<short> sc = Vector256.Create(c1.AsInt16(), c2.AsInt16());

        Vector256<short> pa = sb - sc;

        Vector256<short> pb = sa - sc;

        Vector256<short> pc = Vector256.Abs(pa + pb);

        pa = Vector256.Abs(pa);

        pb = Vector256.Abs(pb);

        Vector256<short> mina = Vector256.LessThanOrEqual(pa, pb) & Vector256.LessThanOrEqual(pa, pc);

        Vector256<short> minb = Vector256.LessThanOrEqual(pb, pc);

        Vector128<byte> maska = Vector128.Narrow(mina.GetLower(), mina.GetUpper()).AsByte();

        Vector128<byte> maskb = Vector128.Narrow(minb.GetLower(), minb.GetUpper()).AsByte();

        maskb = Vector128.AndNot(maskb, maska);

        Vector128<byte> maskc = Vector128.OnesComplement(maska | maskb);

        return a & maska | b & maskb | c & maskc;
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
