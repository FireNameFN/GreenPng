namespace GreenPng;

public readonly struct PngHeader {
    public int Width { get; init; }

    public int Height { get; init; }

    public int BitDepth { get; init; }

    public ImageType ImageType { get; init; }

    public byte CompressionMethod { get; init; }

    public byte FilterMethod { get; init; }

    public byte InterlaceMethod { get; init; }

    public int PixelBitLength { get; init; }

    public int ScanlineLength { get; init; }

    public int ByteSize { get; init; }
}
