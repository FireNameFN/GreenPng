using System;
using System.Buffers;
using System.IO;
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

        int pixelBitLength = imageType switch {
            ImageType.Greyscale => bitDepth,
            ImageType.Truecolor => bitDepth * 3,
            ImageType.IndexedColor => bitDepth,
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

    public static unsafe bool TryDecode(ReadOnlySpan<byte> png, PngHeader header, Span<byte> image) {
        png = png[HeaderLength..];

        fixed(byte* buffer = png) {
            SpanReader reader = new(png);

            SegmentStream data = new(buffer);

            int offset = 8;

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
                        data.Add(offset, chunk.Length);

                        break;
                    case ChunkType.IEND:
                        return TryDecodeData(header, palette, transparency, data, image);
                }

                offset += chunk.Length + 12;
            }

            return false;
        }
    }

    public static PngHeader DecodeHeader(ReadOnlySpan<byte> png) {
        if(!TryDecodeHeader(png, out PngHeader header))
            throw new InvalidOperationException("Header decode error.");

        return header;
    }

    public static void Decode(ReadOnlySpan<byte> png, PngHeader header, Span<byte> image) {
        if(!TryDecode(png, header, image))
            throw new InvalidOperationException("Image decode error.");
    }

    public static byte[] Decode(ReadOnlySpan<byte> png, out PngHeader header) {
        header = DecodeHeader(png);

        byte[] image = new byte[header.ByteSize];

        Decode(png, header, image);

        return image;
    }

    static bool TryDecodeData(PngHeader header, ReadOnlySpan<byte> palette, ReadOnlySpan<byte> transparency, Stream data, Span<byte> image) {
        int filteredLength = header.ScanlineLength + 1;

        int filteredScanlinesLength = filteredLength * header.Height;

        byte[] filteredScanlines = ArrayPool<byte>.Shared.Rent(filteredScanlinesLength);

        using(ZLibStream stream = new(data, CompressionMode.Decompress)) {
            int offset = 0;

            while(offset < filteredScanlinesLength) {
                int length = stream.Read(filteredScanlines.AsSpan(offset));

                if(length == 0) {
                    ArrayPool<byte>.Shared.Return(filteredScanlines);

                    return false;
                }

                offset += length;
            }
        }

        int filterOffset = header.ImageType switch {
            ImageType.Greyscale => 1,
            ImageType.Truecolor => 4,
            ImageType.IndexedColor => 1,
            ImageType.GreyscaleAlpha => 4,
            ImageType.TruecolorAlpha => 4,
            _ => 4
        };

        int stride = (header.Width * filterOffset * header.BitDepth + 7) / 8;

        int imageOffset = (header.Width * 4 - stride) * header.Height;

        Span<byte> scanlines = image[imageOffset..];

        Span<byte> prevScanline = stackalloc byte[stride];

        prevScanline.Clear();

        for(int y = 0; y < header.Height; y++) {
            int filteredOffset = filteredLength * y;

            byte type = filteredScanlines[filteredOffset];

            Span<byte> filteredScanline = filteredScanlines.AsSpan(filteredOffset + 1, header.ScanlineLength);

            Span<byte> scanline = scanlines.Slice(stride * y, stride);

            bool unpacked = false;

            switch(header.ImageType) {
                case ImageType.Truecolor:
                    TruecolorUnpacker.Unpack(filteredScanline, scanline);

                    unpacked = true;

                    break;
                case ImageType.GreyscaleAlpha:
                    GreyscaleAlphaUnpacker.Unpack(filteredScanline, scanline);

                    unpacked = true;

                    break;
            }

            if(unpacked)
                filteredScanline = scanline;

            switch(type) {
                case 0:
                    if(!unpacked)
                        filteredScanline.CopyTo(scanline);

                    break;
                case 1:
                    SubFiltering.Filter(filteredScanline, scanline, filterOffset);
                    break;
                case 2:
                    UpFiltering.Filter(prevScanline, filteredScanline, scanline);
                    break;
                case 3:
                    AverageFiltering.Filter(prevScanline, filteredScanline, scanline, filterOffset);
                    break;
                case 4:
                    PaethFiltering.Filter(prevScanline, filteredScanline, scanline, filterOffset);
                    break;
            }

            prevScanline = scanline;
        }

        ArrayPool<byte>.Shared.Return(filteredScanlines);

        if(header.BitDepth != 8) {
            imageOffset = header.Width * header.Height * (4 - filterOffset);

            Span<byte> deserializedScanlines = image[imageOffset..];

            switch(header.BitDepth) {
                case 1:
                    if(header.ImageType == ImageType.IndexedColor)
                        Deserializer1Bit.Deserialize(scanlines, deserializedScanlines);
                    else
                        Deserializer1Bit.DeserializeScaled(scanlines, deserializedScanlines);

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

        return true;
    }
}
