using System.IO;

namespace GreenPng.Testing;

public static class Resources {
    public static readonly byte[] Truecolor = GetResource("truecolor");

    public static readonly byte[] TruecolorAlpha = GetResource("truecolor_alpha");

    public static readonly byte[] Indexed = GetResource("indexed");

    public static readonly byte[] IndexedAlpha = GetResource("indexed_alpha");

    static byte[] GetResource(string name) {
        using Stream stream = typeof(Resources).Assembly.GetManifestResourceStream($"GreenPng.Testing.Resources.{name}.png");

        byte[] resource = new byte[stream.Length];

        stream.ReadExactly(resource);

        return resource;
    }
}
