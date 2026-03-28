using System.Runtime.Intrinsics;

namespace GreenPng;

public static class Vectors128 {
    public static readonly Vector128<byte> PixelMask = Vector128.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static readonly Vector128<byte> Shift = Vector128.Create((byte)12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);

    public static readonly Vector128<byte>[] MaskArray;

    static Vectors128() {
        MaskArray = new Vector128<byte>[4];

        MaskArray[0] = Vector128.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        for(int i = 0; i < 3; i++)
            MaskArray[i + 1] = Vector128.ShuffleNative(MaskArray[i], Shift);
    }
}
