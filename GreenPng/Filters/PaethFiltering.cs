using System;
using System.Runtime.Intrinsics;

namespace GreenPng.Filters;

public static class PaethFiltering {
    public static void FilterTruecolor(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        int offset = 0;

        Vector128<byte> diagonalScanlineVector = default;

        Vector128<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 15; i += 3) {
            Vector128<byte> prevScanlineVector = Vector128.Create(prevScanline[offset..]);

            Vector128<byte> filteredVector = Vector128.Create(filteredScanline[i..]);

            Vector128<byte> scanlineVector = Vector128.ShuffleNative(filteredVector, Filtering.Shuffle128);

            scanlineVector += Paeth(subScanlineVector, prevScanlineVector, diagonalScanlineVector) & Filtering.PixelMask128;

            scanlineVector |= Filtering.MaskAlpha128;

            scanlineVector.CopyTo(scanline[offset..]);

            diagonalScanlineVector = prevScanlineVector;

            subScanlineVector = scanlineVector;

            offset += 4;
        }

        if(i < 1) {
            scanline[0] = (byte)(filteredScanline[2] + Paeth(0, scanline[0], 0));
            scanline[1] = (byte)(filteredScanline[1] + Paeth(0, scanline[1], 0));
            scanline[2] = (byte)(filteredScanline[0] + Paeth(0, scanline[2], 0));
            scanline[3] = 0xFF;

            i = 3;
            offset = 4;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[offset] = (byte)(filteredScanline[i + 2] + Paeth(scanline[offset - 4], prevScanline[offset], prevScanline[offset - 4]));
            scanline[offset + 1] = (byte)(filteredScanline[i + 1] + Paeth(scanline[offset - 3], prevScanline[offset + 1], prevScanline[offset - 3]));
            scanline[offset + 2] = (byte)(filteredScanline[i] + Paeth(scanline[offset - 2], prevScanline[offset + 2], prevScanline[offset - 2]));
            scanline[offset + 3] = 0xFF;

            offset += 4;
        }
    }

    public static void FilterTruecolorAlpha(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, Span<byte> scanline) {
        int i = 0;

        Vector256<byte> subScanlineVector = default;

        for(; i < filteredScanline.Length - 31; i += 32) {
            Vector256<byte> prevScanlineVector = Vector256.Create(prevScanline[i..]);

            Vector256<byte> filteredVector = Vector256.Create(filteredScanline[i..]);

            Vector256<byte> scanlineVector = Vector256.ShuffleNative(filteredVector, Filtering.ShuffleAlpha256);

            for(int j = 0; j < 8; j++) {
                Vector256<byte> subVector = subScanlineVector;

                //if(j > 0)
                //    subVector = Vector256.ShuffleNative(scanlineVector, Shift);

                Vector256<byte> and = prevScanlineVector & subVector;

                Vector256<byte> xor = (prevScanlineVector ^ subVector) >> 1;

                //scanlineVector += (and + xor) & MaskArray[j];
            }

            subScanlineVector = Vector256.ShuffleNative(scanlineVector, Filtering.LastShift256) & Filtering.LastShiftMask256;

            scanlineVector.CopyTo(scanline[i..]);
        }

        if(i < 1) {
            scanline[0] = (byte)(filteredScanline[2] + scanline[0] / 2);
            scanline[1] = (byte)(filteredScanline[1] + scanline[1] / 2);
            scanline[2] = (byte)(filteredScanline[0] + scanline[2] / 2);
            scanline[3] = 0xFF;
        }

        for(; i < filteredScanline.Length; i += 3) {
            scanline[i] = (byte)(filteredScanline[i + 2] + (scanline[i - 4] + prevScanline[i]) / 2);
            scanline[i + 1] = (byte)(filteredScanline[i + 1] + (scanline[i - 3] + prevScanline[i + 1]) / 2);
            scanline[i + 2] = (byte)(filteredScanline[i] + (scanline[i - 2] + prevScanline[i + 2]) / 2);
            scanline[i + 3] = (byte)(filteredScanline[i + 3] + (scanline[i - 1] + prevScanline[i + 3]) / 2);
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
