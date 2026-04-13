using System.Runtime.Intrinsics;

namespace GreenPng.Processing;

public static class Vectors256 {
    public static readonly Vector256<byte> Shuffle = Vector256.Create((byte)2, 1, 0, 3, 6, 5, 4, 7, 10, 9, 8, 11, 14, 13, 12, 15, 18, 17, 16, 19, 22, 21, 20, 23, 26, 25, 24, 27, 30, 29, 28, 31);

    public static readonly Vector256<byte> ShuffleLane = Vector256.Create((byte)2, 1, 0, 3, 5, 4, 3, 7, 8, 7, 6, 11, 11, 10, 9, 15, 22, 21, 20, 19, 25, 24, 23, 23, 28, 27, 26, 27, 31, 30, 29, 31);

    public static readonly Vector256<byte> ShuffleMono = Vector256.Create((byte)0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 20, 20, 20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 23);

    public static readonly Vector256<byte> Shuffle1Bit = Vector256.Create((byte)0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19);

    public static readonly Vector256<byte> Shuffle2Bit = Vector256.Create((byte)0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 20, 20, 20, 20, 21, 21, 21, 21, 22, 22, 22, 22, 23, 23, 23, 23);

    public static readonly Vector256<byte> MaskMono = Vector256.Create(255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0);

    public static readonly Vector256<byte> MaskAlpha = Vector256.Create(0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255);

    public static readonly Vector256<byte> Mask1Bit = Vector256.Create(128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1);

    public static readonly Vector256<byte> Mask2BitHigh = Vector256.Create(128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2, 128, 32, 8, 2);

    public static readonly Vector256<byte> Mask2BitLow = Vector256.Create((byte)64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1, 64, 16, 4, 1);

    public static readonly Vector256<byte> MaskBit12 = Vector256.Create((byte)0b10101010);

    public static readonly Vector256<byte> MaskBit1 = Vector256.Create((byte)1);

    public static readonly Vector256<byte> MaskBit2 = Vector256.Create((byte)2);
}
