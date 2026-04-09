namespace GreenPng.Testing;

public static class TestImageDataSource {
    static readonly TestImage Greyscale = new("Greyscale", Resources.Greyscale);

    static readonly TestImage GreyscaleAlpha = new("Greyscale Alpha", Resources.GreyscaleAlpha);

    static readonly TestImage Truecolor = new("Truecolor", Resources.Truecolor);

    static readonly TestImage TruecolorAlpha = new("Truecolor Alpha", Resources.TruecolorAlpha);

    static readonly TestImage Indexed = new("Indexed", Resources.Indexed);

    static readonly TestImage IndexedFiltered = new("Indexed Filtered", Resources.IndexedFiltered);

    static readonly TestImage IndexedAlpha = new("Indexed Alpha", Resources.IndexedAlpha);

    public static TestImage[] GetTestImages() => [Greyscale, GreyscaleAlpha, Truecolor, TruecolorAlpha, Indexed, IndexedFiltered, IndexedAlpha];
}
