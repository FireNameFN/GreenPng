namespace GreenPng.Testing;

public static class TestImageDataSource {
    public static readonly TestImage Greyscale = new("Greyscale", Resources.Greyscale);

    public static readonly TestImage GreyscaleAlpha = new("Greyscale Alpha", Resources.GreyscaleAlpha);

    public static readonly TestImage Greyscale1Bit = new("Greyscale 1 Bit", Resources.Greyscale1Bit);

    public static readonly TestImage Greyscale2Bit = new("Greyscale 2 Bit", Resources.Greyscale2Bit);

    public static readonly TestImage Greyscale4Bit = new("Greyscale 4 Bit", Resources.Greyscale4Bit);

    public static readonly TestImage Truecolor = new("Truecolor", Resources.Truecolor);

    public static readonly TestImage TruecolorAlpha = new("Truecolor Alpha", Resources.TruecolorAlpha);

    public static readonly TestImage Indexed = new("Indexed", Resources.Indexed);

    public static readonly TestImage IndexedFiltered = new("Indexed Filtered", Resources.IndexedFiltered);

    public static readonly TestImage IndexedAlpha = new("Indexed Alpha", Resources.IndexedAlpha);

    public static readonly TestImage Indexed1Bit = new("Indexed 1 Bit", Resources.Indexed1Bit);

    public static readonly TestImage Indexed2Bit = new("Indexed 2 Bit", Resources.Indexed2Bit);

    public static readonly TestImage Indexed4Bit = new("Indexed 4 Bit", Resources.Indexed4Bit);

    public static TestImage[] GetTestImages() => [Greyscale, GreyscaleAlpha, Greyscale1Bit, Greyscale2Bit, Greyscale4Bit, Truecolor, TruecolorAlpha, Indexed, IndexedFiltered, IndexedAlpha, Indexed1Bit, Indexed2Bit, Indexed4Bit];
}
