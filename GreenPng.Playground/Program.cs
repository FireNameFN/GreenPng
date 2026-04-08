using System;
using GreenPng;
using GreenPng.Testing;

byte[] png = Resources.Greyscale;

PngDecoder.TryDecodeHeader(png, out PngHeader header);

Console.WriteLine($"{header.Width}x{header.Height}");
Console.WriteLine($"{header.BitDepth} bits");
Console.WriteLine(header.ImageType);
Console.WriteLine(header.CompressionMethod);
Console.WriteLine(header.FilterMethod);
Console.WriteLine(header.InterlaceMethod);
Console.WriteLine(header.ByteSize);

Span<byte> span = stackalloc byte[header.ByteSize];

bool result = PngDecoder.TryDecode(png, header, span);

result = Decode(span);

Console.WriteLine(result);

bool Decode(Span<byte> span) {
    return PngDecoder.TryDecode(png, header, span);
}
