using System.IO;

namespace GreenPng.Testing;

public static class Resources {
    public static readonly byte[] Greyscale = GetResource("greyscale");

    public static readonly byte[] GreyscaleAlpha = GetResource("greyscale_alpha");

    public static readonly byte[] Greyscale1Bit = GetResource("greyscale_1bit");

    public static readonly byte[] Greyscale2Bit = GetResource("greyscale_2bit");

    public static readonly byte[] Truecolor = GetResource("truecolor");

    public static readonly byte[] TruecolorAlpha = GetResource("truecolor_alpha");

    public static readonly byte[] Indexed = GetResource("indexed");

    public static readonly byte[] IndexedFiltered = GetResource("indexed_filtered");

    public static readonly byte[] IndexedAlpha = GetResource("indexed_alpha");

    public static readonly byte[] Indexed1Bit = GetResource("indexed_1bit");

    public static readonly byte[] Indexed2Bit = GetResource("indexed_2bit");

    static byte[] GetResource(string name) {
        using Stream stream = typeof(Resources).Assembly.GetManifestResourceStream($"GreenPng.Testing.Resources.{name}.png");

        byte[] resource = new byte[stream.Length];

        stream.ReadExactly(resource);

        return resource;
    }
}
