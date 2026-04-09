namespace GreenPng.Testing;

public static class TestImageDataSource {
    static readonly TestImage Greyscale = new("Greyscale", Resources.Greyscale);

    static readonly TestImage GreyscaleAlpha = new("Greyscale Alpha", Resources.GreyscaleAlpha);

    static readonly TestImage Greyscale1Bit = new("Greyscale 1 Bit", Resources.Greyscale1Bit);

    static readonly TestImage Truecolor = new("Truecolor", Resources.Truecolor);

    static readonly TestImage TruecolorAlpha = new("Truecolor Alpha", Resources.TruecolorAlpha);

    static readonly TestImage Indexed = new("Indexed", Resources.Indexed);

    static readonly TestImage IndexedFiltered = new("Indexed Filtered", Resources.IndexedFiltered);

    static readonly TestImage IndexedAlpha = new("Indexed Alpha", Resources.IndexedAlpha);

    static readonly TestImage Indexed1Bit = new("Indexed 1 Bit", Resources.Indexed1Bit);

    public static TestImage[] GetTestImages() => [Greyscale, GreyscaleAlpha, Greyscale1Bit, Truecolor, TruecolorAlpha, Indexed, IndexedFiltered, IndexedAlpha, Indexed1Bit];
}
