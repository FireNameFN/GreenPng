using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using GreenBuf;
using GreenPng.Filters;

namespace GreenPng;

public static class PngDecoder {
    const int HeaderLength = 33;

    static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static bool TryDecodeHeader(ReadOnlySpan<byte> png, out PngHeader header) {
        header = default;

        if(png.Length < HeaderLength)
            return false;

        scoped SpanReader reader = new(png);

        ReadOnlySpan<byte> signature = reader.Get(8);

        if(!signature.SequenceEqual(PngSignature))
            return false;

        if(!reader.TryGetChunk(out ChunkType type, out ReadOnlySpan<byte> chunk))
            return false;

        if(type != ChunkType.IHDR)
            return false;

        reader = new(chunk);

        int width = reader.GetInt32();

        int height = reader.GetInt32();

        int bitDepth = reader.GetByte();

        ImageType imageType = (ImageType)reader.GetByte();

        byte compressionMethod = reader.GetByte();

        byte filterMethod = reader.GetByte();

        byte interlaceMethod = reader.GetByte();

        int pixelBitLength = imageType switch {
            ImageType.Greyscale => bitDepth,
            ImageType.Truecolor => bitDepth * 3,
            ImageType.IndexedColor => 8,
            ImageType.GreyscaleAlpha => bitDepth * 2,
            ImageType.TruecolorAlpha => bitDepth * 4,
            _ => 0
        };

        int scanlineLength = (width * pixelBitLength + 7) / 8;

        int size = width * height * 4;

        header = new() {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ImageType = imageType,
            CompressionMethod = compressionMethod,
            FilterMethod = filterMethod,
            InterlaceMethod = interlaceMethod,
            PixelBitLength = pixelBitLength,
            ScanlineLength = scanlineLength,
            ByteSize = size
        };

        return true;
    }

    public static bool TryDecode(ReadOnlySpan<byte> png, PngHeader header, Span<byte> image) {
        SpanReader reader = new(png[HeaderLength..]);

        byte[] data = ArrayPool<byte>.Shared.Rent(reader.Length);

        SpanWriter writer = new(data);

        scoped ReadOnlySpan<byte> palette = default;

        scoped ReadOnlySpan<byte> transparency = default;

        while(reader.TryGetChunk(out ChunkType type, out ReadOnlySpan<byte> chunk)) {
            switch(type) {
                case ChunkType.PLTE:
                    palette = chunk;

                    break;
                case ChunkType.tRNS:
                    transparency = chunk;

                    break;
                case ChunkType.IDAT:
                    writer.Write(chunk);

                    break;
                case ChunkType.IEND:
                    bool success = TryDecodeData(header, palette, transparency, data, image);

                    ArrayPool<byte>.Shared.Return(data);

                    return success;
            }
        }

        ArrayPool<byte>.Shared.Return(data);

        return false;
    }

    public static byte[] Decode(ReadOnlySpan<byte> png, out PngHeader header) {
        if(!TryDecodeHeader(png, out header))
            throw new InvalidOperationException("Header decode error.");

        byte[] image = new byte[header.ByteSize];

        if(!TryDecode(png, header, image))
            throw new InvalidOperationException("Image decode error.");

        return image;
    }

    static bool TryDecodeData(PngHeader header, ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, byte[] data, Span<byte> image) {
        int filteredLength = header.ScanlineLength + 1;

        int filteredScanlinesLength = filteredLength * header.Height;

        byte[] filteredScanlines = ArrayPool<byte>.Shared.Rent(filteredScanlinesLength);

        using(ZLibStream stream = new(new MemoryStream(data), CompressionMode.Decompress)) {
            int offset = 0;

            while(offset < filteredScanlinesLength) {
                int length = stream.Read(filteredScanlines.AsSpan(offset));

                if(length == 0)
                    return false;

                offset += length;
            }
        }

        Span<byte> reversedPalette = stackalloc byte[1024];

        int offset1 = 0;

        for(int i = 0; i < palette.Length; i += 3) {
            reversedPalette[offset1] = palette[i + 2];
            reversedPalette[offset1 + 1] = palette[i + 1];
            reversedPalette[offset1 + 2] = palette[i];

            offset1 += 4;
        }

        Span<uint> reversedPalettePixel = MemoryMarshal.Cast<byte, uint>(reversedPalette);

        Span<uint> imagePixel = MemoryMarshal.Cast<byte, uint>(image);

        Span<uint> prevScanline = stackalloc uint[header.Width];

        for(int y = 0; y < header.Height; y++) {
            Span<uint> scanline = imagePixel.Slice(header.Width * y, header.Width);

            int filteredOffset = filteredLength * y;

            byte type = filteredScanlines[filteredOffset];

            ReadOnlySpan<byte> filteredScanline = filteredScanlines.AsSpan(filteredOffset + 1, header.ScanlineLength);

            switch(header.ImageType) {
                case ImageType.Greyscale:
                    break;
                case ImageType.Truecolor:
                    DecodeTruecolor(prevScanline, filteredScanline, type, scanline);
                    break;
                case ImageType.IndexedColor:
                    if(transparency.Length < 1)
                        DecodeIndexed(reversedPalettePixel, filteredScanline, scanline);
                    else
                        DecodeIndexedAlpha(reversedPalettePixel, transparency, filteredScanline, scanline);

                    break;
                case ImageType.GreyscaleAlpha:
                    break;
                case ImageType.TruecolorAlpha:
                    DecodeTruecolorAlpha(prevScanline, filteredScanline, type, scanline);
                    break;
            }

            prevScanline = scanline;
        }

        ArrayPool<byte>.Shared.Return(filteredScanlines);

        return true;
    }

    static void DecodeTruecolor(ReadOnlySpan<uint> prevScanline, ReadOnlySpan<byte> filteredScanline, byte type, Span<uint> scanline) {
        ReadOnlySpan<byte> prevScanlineByte = MemoryMarshal.AsBytes(prevScanline);
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);

        switch(type) {
            case 0:
                NoneFiltering.FilterTruecolor(filteredScanline, scanlineByte);
                break;
            case 1:
                SubFiltering.FilterTruecolor(filteredScanline, scanlineByte);
                break;
            case 2:
                UpFiltering.FilterTruecolor(prevScanlineByte, filteredScanline, scanlineByte);
                break;
            case 3:
                AverageFiltering.FilterTruecolor(prevScanlineByte, filteredScanline, scanlineByte);
                break;
            case 4:
                PaethFiltering.FilterTruecolor(prevScanlineByte, filteredScanline, scanlineByte);
                //Filtering.FilterPaeth(prevScanline, filteredScanline, scanline);
                break;
        }
    }

    static void DecodeTruecolorAlpha(ReadOnlySpan<uint> prevScanline, ReadOnlySpan<byte> filteredScanline, byte type, Span<uint> scanline) {
        ReadOnlySpan<byte> prevScanlineByte = MemoryMarshal.AsBytes(prevScanline);
        Span<byte> scanlineByte = MemoryMarshal.AsBytes(scanline);

        switch(type) {
            case 0:
                NoneFiltering.FilterTruecolorAlpha(filteredScanline, scanlineByte);
                break;
            case 1:
                SubFiltering.FilterTruecolorAlpha(filteredScanline, scanlineByte);
                break;
            case 2:
                UpFiltering.FilterTruecolorAlpha(prevScanlineByte, filteredScanline, scanlineByte);
                break;
            case 3:
                AverageFiltering.FilterTruecolorAlpha(prevScanlineByte, filteredScanline, scanlineByte);
                break;
            case 4:
                PaethFiltering.FilterTruecolor(prevScanlineByte, filteredScanline, scanlineByte);
                break;
        }
    }

    static void DecodeIndexed(ReadOnlySpan<uint> reversedPalettePixel, ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        for(int x = 0; x < scanline.Length; x++) {
            int index = filteredScanline[x];

            uint pixel = reversedPalettePixel[index];

            pixel |= 0xFF000000;

            scanline[x] = pixel;
        }
    }

    static void DecodeIndexedAlpha(ReadOnlySpan<uint> reversedPalettePixel, ReadOnlySpan<byte> transparency, ReadOnlySpan<byte> filteredScanline, Span<uint> scanline) {
        for(int x = 0; x < scanline.Length; x++) {
            int index = filteredScanline[x];

            uint pixel = reversedPalettePixel[index];

            pixel |= (uint)transparency[index] << 24;

            scanline[x] = pixel;
        }
    }
}
