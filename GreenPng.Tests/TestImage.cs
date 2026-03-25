namespace GreenPng.Tests;

public readonly struct TestImage(string name, byte[] png) {
    public string Name { get; init; } = name;

    public byte[] Png { get; init; } = png;

    public override string ToString() {
        return Name;
    }
}
