using System;
using GreenPng.Filters;

namespace GreenPng.Decoders;

public sealed class TruecolorDecoder {
    public static void Decode(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, byte type, Span<byte> scanline) {
        switch(type) {
            case 0:
                NoneFiltering.Filter(filteredScanline, scanline);
                break;
            case 1:
                SubFiltering.Filter(filteredScanline, scanline);
                break;
            case 2:
                UpFiltering.Filter(prevScanline, filteredScanline, scanline);
                break;
            case 3:
                AverageFiltering.Filter(prevScanline, filteredScanline, scanline);
                break;
            case 4:
                PaethFiltering.Filter(prevScanline, filteredScanline, scanline);
                break;
        }
    }

    public static void DecodeAlpha(ReadOnlySpan<byte> prevScanline, ReadOnlySpan<byte> filteredScanline, byte type, Span<byte> scanline) {
        switch(type) {
            case 0:
                NoneFiltering.FilterAlpha(filteredScanline, scanline);
                break;
            case 1:
                SubFiltering.FilterAlpha(filteredScanline, scanline);
                break;
            case 2:
                UpFiltering.FilterAlpha(prevScanline, filteredScanline, scanline);
                break;
            case 3:
                AverageFiltering.FilterAlpha(prevScanline, filteredScanline, scanline);
                break;
            case 4:
                PaethFiltering.FilterAlpha(prevScanline, filteredScanline, scanline);
                break;
        }
    }
}
