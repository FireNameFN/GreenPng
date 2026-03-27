using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using GreenBuf;
using GreenPng.Decoders;

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

        int stride = header.Width * 4;

        Span<byte> prevScanline = stackalloc byte[stride];

        for(int y = 0; y < header.Height; y++) {
            Span<byte> scanline = image.Slice(stride * y, stride);

            int filteredOffset = filteredLength * y;

            byte type = filteredScanlines[filteredOffset];

            ReadOnlySpan<byte> filteredScanline = filteredScanlines.AsSpan(filteredOffset + 1, header.ScanlineLength);

            switch(header.ImageType) {
                case ImageType.Greyscale:
                    break;
                case ImageType.Truecolor:
                    TruecolorDecoder.Decode(prevScanline, filteredScanline, type, scanline);
                    break;
                case ImageType.IndexedColor:
                    if(transparency.Length < 1)
                        IndexedDecoder.Decode2(palette, filteredScanline, scanline);
                    else
                        IndexedDecoder.DecodeAlpha(palette, transparency, filteredScanline, scanline);

                    break;
                case ImageType.GreyscaleAlpha:
                    break;
                case ImageType.TruecolorAlpha:
                    TruecolorDecoder.DecodeAlpha(prevScanline, filteredScanline, type, scanline);
                    break;
            }

            prevScanline = scanline;
        }

        ArrayPool<byte>.Shared.Return(filteredScanlines);

        return true;
    }
}
