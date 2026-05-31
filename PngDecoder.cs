using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using GreenBuf;
using GreenPng.Processing;
using GreenPng.Processing.Decoders;
using GreenPng.Processing.Deserializers;
using GreenPng.Processing.Filters;
using GreenPng.Processing.Unpackers;

namespace GreenPng;

public static class PngDecoder {
    const int HeaderLength = 33;

    static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static bool IsHeaderSupported(PngHeader header) {
        if(header.Width < 1)
            return false;

        if(header.Height < 1)
            return false;

        if(header.BitDepth is not (1 or 2 or 4 or 8))
            return false;

        if(header.ImageType is not (ImageType.Greyscale or ImageType.Truecolor or ImageType.IndexedColor or ImageType.GreyscaleAlpha or ImageType.TruecolorAlpha))
            return false;

        if(header.CompressionMethod != 0)
            return false;

        if(header.FilterMethod != 0)
            return false;

        if(header.InterlaceMethod != 0)
            return false;

        return true;
    }

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

        if(chunk.Length < 13)
            return false;

        reader = new(chunk);

        int width = reader.GetInt32();

        int height = reader.GetInt32();

        int bitDepth = reader.GetByte();

        ImageType imageType = (ImageType)reader.GetByte();

        byte compressionMethod = reader.GetByte();

        byte filterMethod = reader.GetByte();

        byte interlaceMethod = reader.GetByte();

        int size = width * height * 4;

        header = new() {
            Width = width,
            Height = height,
            BitDepth = bitDepth,
            ImageType = imageType,
            CompressionMethod = compressionMethod,
            FilterMethod = filterMethod,
            InterlaceMethod = interlaceMethod,
            ByteSize = size
        };

        return true;
    }

    public static bool TryDecode(ReadOnlySpan<byte> png, PngHeader header, Span<byte> image) {
        int packedOffset = header.ImageType switch {
            ImageType.Greyscale => 1,
            ImageType.Truecolor => 3,
            ImageType.IndexedColor => 1,
            ImageType.GreyscaleAlpha => 2,
            ImageType.TruecolorAlpha => 4,
            _ => 0
        };

        int scanlineLength = (header.Width * packedOffset * header.BitDepth + 7) >> 3;

        int packedStride = scanlineLength + 1;

        int packedScanlinesLength = packedStride * header.Height;

        int stride = header.ImageType switch {
            ImageType.Truecolor => header.Width * 4,
            ImageType.GreyscaleAlpha => header.Width * 4,
            _ => scanlineLength
        };

        ZLibDecoder decoder = new();

        byte[] packedScanlines = ArrayPool<byte>.Shared.Rent(packedScanlinesLength + stride);

        bool ok = TryDecodeData(png, header, decoder, packedScanlines, packedScanlinesLength, stride, packedStride, packedOffset, image);

        ArrayPool<byte>.Shared.Return(packedScanlines);

        decoder.Dispose();

        return ok;
    }

    public static bool TryDecode(ReadOnlySpan<byte> png, PngHeader header, out byte[] image) {
        image = GC.AllocateUninitializedArray<byte>(header.ByteSize);

        return TryDecode(png, header, image);
    }

    public static bool TryDecode(ReadOnlySpan<byte> png, out PngHeader header, [NotNullWhen(true)] out byte[]? image) {
        image = null;

        if(!TryDecodeHeader(png, out header))
            return false;

        if(!IsHeaderSupported(header))
            return false;

        if(!TryDecode(png, header, out image))
            return false;

        return true;
    }

    public static byte[] Decode(ReadOnlySpan<byte> png, out PngHeader header) {
        if(!TryDecodeHeader(png, out header))
            throw new InvalidOperationException("Header decode error.");

        if(!IsHeaderSupported(header))
            throw new InvalidOperationException("Header is not supported.");

        if(!TryDecode(png, header, out byte[] image))
            throw new InvalidOperationException("Image decode error.");

        return image;
    }

    static bool TryDecodeData(ReadOnlySpan<byte> png, PngHeader header, ZLibDecoder decoder, Span<byte> packedScanlines, int packedScanlinesLength, int stride, int packedStride, int packedOffset, Span<byte> image) {
        SpanReader reader = new(png[HeaderLength..]);

        int offset = 0;

        scoped ReadOnlySpan<byte> palette = default;

        scoped ReadOnlySpan<byte> transparency = default;

        while(reader.TryGetChunk(out ChunkType type, out ReadOnlySpan<byte> chunk)) {
            switch(type) {
                case ChunkType.PLTE:
                    if(chunk.Length > (1 << header.BitDepth) * 3)
                        return false;

                    palette = chunk;

                    break;
                case ChunkType.tRNS:
                    if(chunk.Length * 3 > palette.Length)
                        return false;

                    transparency = chunk;

                    break;
                case ChunkType.IDAT:
                    decoder.Decompress(chunk, packedScanlines[offset..], out _, out int advance);

                    offset += advance;

                    break;
                case ChunkType.IEND:
                    if(offset != packedScanlinesLength)
                        return false;

                    DecodeScanlines(header, palette, transparency, packedScanlines, stride, packedStride, packedOffset, image);

                    return true;
            }
        }

        return false;
    }

    static void DecodeScanlines(PngHeader header, ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, Span<byte> packedScanlines, int stride, int packedStride, int packedOffset, Span<byte> image) {
        int imageOffset = (header.Width * 4 - stride) * header.Height;

        Span<byte> scanlines = image[imageOffset..];

        switch(header.ImageType) {
            case ImageType.Truecolor:
            case ImageType.GreyscaleAlpha:
                UnpackFilterImage(header, packedScanlines, stride, packedStride, scanlines);
                break;
            default:
                FilterImage(header, packedScanlines, stride, packedStride, packedOffset, scanlines);
                break;
        }

        DecodeImage(header, palette, transparency, scanlines, packedOffset, image);
    }

    static void FilterImage(PngHeader header, Span<byte> packedScanlines, int stride, int packedStride, int offset, Span<byte> scanlines) {
        Span<byte> prevScanline = packedScanlines[^stride..];

        prevScanline.Clear();

        for(int y = 0; y < header.Height; y++) {
            int packedOffset = packedStride * y;

            byte type = packedScanlines[packedOffset];

            ReadOnlySpan<byte> packedScanline = packedScanlines.Slice(packedOffset + 1, packedStride - 1);

            Span<byte> scanline = scanlines.Slice(stride * y, stride);

            switch(type) {
                case 0:
                    packedScanline.CopyTo(scanline);
                    break;
                case 1:
                    SubFiltering.Filter(packedScanline, scanline, offset);
                    break;
                case 2:
                    UpFiltering.Filter(prevScanline, packedScanline, scanline);
                    break;
                case 3:
                    AverageFiltering.Filter(prevScanline, packedScanline, scanline, offset);
                    break;
                case 4:
                    PaethFiltering.Filter(prevScanline, packedScanline, scanline, offset);
                    break;
            }

            prevScanline = scanline;
        }
    }

    static void UnpackFilterImage(PngHeader header, Span<byte> packedScanlines, int stride, int packedStride, Span<byte> scanlines) {
        Span<byte> prevScanline = packedScanlines[^stride..];

        prevScanline.Clear();

        for(int y = 0; y < header.Height; y++) {
            int packedOffset = packedStride * y;

            byte type = packedScanlines[packedOffset];

            ReadOnlySpan<byte> packedScanline = packedScanlines.Slice(packedOffset + 1, packedStride - 1);

            Span<byte> scanline = scanlines.Slice(stride * y, stride);

            switch(header.ImageType) {
                case ImageType.Truecolor:
                    TruecolorUnpacker.Unpack(packedScanline, scanline);
                    break;
                case ImageType.GreyscaleAlpha:
                    GreyscaleAlphaUnpacker.Unpack(packedScanline, scanline);
                    break;
            }

            switch(type) {
                case 1:
                    SubFiltering.Filter(scanline, scanline, 4);
                    break;
                case 2:
                    UpFiltering.Filter(prevScanline, scanline, scanline);
                    break;
                case 3:
                    AverageFiltering.Filter(prevScanline, scanline, scanline, 4);
                    break;
                case 4:
                    PaethFiltering.Filter(prevScanline, scanline, scanline, 4);
                    break;
            }

            prevScanline = scanline;
        }
    }

    static void DecodeImage(PngHeader header, ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, ReadOnlySpan<byte> scanlines, int filterOffset, Span<byte> image) {
        if(header.BitDepth != 8) {
            int imageOffset = header.Width * header.Height * (4 - filterOffset);

            Span<byte> deserializedScanlines = image[imageOffset..];

            switch((header.ImageType, header.BitDepth)) {
                case (ImageType.IndexedColor, 1):
                    Deserializer1Bit.Deserialize(scanlines, deserializedScanlines);
                    break;
                case (ImageType.IndexedColor, 2):
                    Deserializer2Bit.Deserialize(scanlines, deserializedScanlines);
                    break;
                case (ImageType.IndexedColor, 4):
                    Deserializer4Bit.Deserialize(scanlines, deserializedScanlines);
                    break;
                case (ImageType.Greyscale, 1):
                    Deserializer1Bit.DeserializeScaled(scanlines, deserializedScanlines);
                    break;
                case (ImageType.Greyscale, 2):
                    Deserializer2Bit.DeserializeScaled(scanlines, deserializedScanlines);
                    break;
                case (ImageType.Greyscale, 4):
                    Deserializer4Bit.DeserializeScaled(scanlines, deserializedScanlines);
                    break;
            }

            scanlines = deserializedScanlines;
        }

        switch(header.ImageType) {
            case ImageType.Greyscale:
                GreyscaleDecoder.Decode(scanlines, image);
                break;
            case ImageType.Truecolor:
                TruecolorDecoder.Decode(image);
                break;
            case ImageType.IndexedColor:
                IndexedDecoder.Decode(palette, transparency, scanlines, image);
                break;
            case ImageType.TruecolorAlpha:
                TruecolorAlphaDecoder.Decode(image);
                break;
        }
    }
}
