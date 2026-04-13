namespace GreenPng.Testing;

public static class TestImageDataSource {
    static readonly TestImage Greyscale = new("Greyscale", Resources.Greyscale);

    static readonly TestImage GreyscaleAlpha = new("Greyscale Alpha", Resources.GreyscaleAlpha);

    static readonly TestImage Greyscale1Bit = new("Greyscale 1 Bit", Resources.Greyscale1Bit);

    static readonly TestImage Greyscale2Bit = new("Greyscale 2 Bit", Resources.Greyscale2Bit);

    static readonly TestImage Greyscale4Bit = new("Greyscale 4 Bit", Resources.Greyscale4Bit);

    static readonly TestImage Truecolor = new("Truecolor", Resources.Truecolor);

    static readonly TestImage TruecolorAlpha = new("Truecolor Alpha", Resources.TruecolorAlpha);

    static readonly TestImage Indexed = new("Indexed", Resources.Indexed);

    static readonly TestImage IndexedFiltered = new("Indexed Filtered", Resources.IndexedFiltered);

    static readonly TestImage IndexedAlpha = new("Indexed Alpha", Resources.IndexedAlpha);

    static readonly TestImage Indexed1Bit = new("Indexed 1 Bit", Resources.Indexed1Bit);

    static readonly TestImage Indexed2Bit = new("Indexed 2 Bit", Resources.Indexed2Bit);

    static readonly TestImage Indexed4Bit = new("Indexed 4 Bit", Resources.Indexed4Bit);

    public static TestImage[] GetTestImages() => [Greyscale, GreyscaleAlpha, Greyscale1Bit, Greyscale2Bit, Greyscale4Bit, Truecolor, TruecolorAlpha, Indexed, IndexedFiltered, IndexedAlpha, Indexed1Bit, Indexed2Bit, Indexed4Bit];
}
