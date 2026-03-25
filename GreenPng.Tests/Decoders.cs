using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GreenPng.Tests;

public static class Decoders {
    public static byte[] DecodeGreenPng(byte[] png) {
        return PngDecoder.Decode(png, out _);
    }

    public static byte[] DecodeImageMagick(byte[] png) {
        using MagickImage magick = new(png);

        return magick.ToByteArray(MagickFormat.Bgra);
    }

    public static byte[] DecodeStbImageSharpBgra(byte[] png) {
        byte[] image = StbImageSharp.ImageResult.FromMemory(png, StbImageSharp.ColorComponents.RedGreenBlueAlpha).Data;

        for(int i = 0; i < image.Length; i += 4)
            (image[i], image[i + 2]) = (image[i + 2], image[i]);

        return image;
    }

    public static byte[] DecodeStbImageSharpRgba(byte[] png) {
        return StbImageSharp.ImageResult.FromMemory(png, StbImageSharp.ColorComponents.RedGreenBlueAlpha).Data;
    }

    public static byte[] DecodeImageSharp(byte[] png) {
        using Image<Bgra32> sharp = Image.Load<Bgra32>(png);

        byte[] image = new byte[sharp.Width * sharp.Height * 4];

        sharp.CopyPixelDataTo(image);

        return image;
    }
}
