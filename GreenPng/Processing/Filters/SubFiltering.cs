using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace GreenPng.Processing.Filters;

public static class SubFiltering {
    public static void Filter(Span<byte> scanline, int offset) {
        if(offset == 4 && Sse2.IsSupported) {
            FilterSse2(scanline);

            return;
        }

        FilterScalar(scanline, offset);
    }

    public static void FilterSse2(Span<byte> scanline) {
        Span<int> scanlineInt32 = MemoryMarshal.Cast<byte, int>(scanline);

        Vector128<byte> subScanlineVector = default;

        for(int i = 0; i < scanlineInt32.Length; i++) {
            Vector128<byte> scanlineVector = Sse2.ConvertScalarToVector128Int32(scanlineInt32[i]).AsByte();

            subScanlineVector = Sse2.Add(scanlineVector, subScanlineVector);

            scanlineInt32[i] = Sse2.ConvertToInt32(subScanlineVector.AsInt32());
        }
    }

    public static void FilterScalar(Span<byte> scanline, int offset) {
        for(int i = 4; i < scanline.Length; i++)
            scanline[i] += scanline[i - offset];
    }
}
