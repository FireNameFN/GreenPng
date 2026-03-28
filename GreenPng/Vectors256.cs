using System.Runtime.Intrinsics;

namespace GreenPng;

public static class Vectors256 {
    public static readonly Vector256<byte> Shuffle = Vector256.Create((byte)2, 1, 0, 3, 5, 4, 3, 7, 8, 7, 6, 11, 11, 10, 9, 15, 14, 13, 12, 19, 17, 16, 15, 23, 20, 19, 18, 27, 23, 22, 21, 31);

    public static readonly Vector256<byte> ShuffleAlpha = Vector256.Create((byte)2, 1, 0, 3, 6, 5, 4, 7, 10, 9, 8, 11, 14, 13, 12, 15, 18, 17, 16, 19, 22, 21, 20, 23, 26, 25, 24, 27, 30, 29, 28, 31);

    public static readonly Vector256<byte> ShuffleMono = Vector256.Create((byte)0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7);

    public static readonly Vector256<byte> MaskMono = Vector256.Create(255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0);

    public static readonly Vector256<byte> MaskAlpha = Vector256.Create(0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255);

    public static readonly Vector256<byte> LastShift = Vector256.Create((byte)28, 29, 30, 31, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);

    public static readonly Vector256<byte> LastShiftMask = Vector256.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static readonly Vector256<byte>[] ShiftArray;

    public static readonly Vector256<byte>[] ShiftMaskArray;

    static Vectors256() {
        ShiftArray = new Vector256<byte>[3];
        
        ShiftArray[0] = Vector256.Create((byte)0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27);

        for(int i = 0; i < 2; i++)
            ShiftArray[i + 1] = Vector256.ShuffleNative(ShiftArray[i], ShiftArray[i]);

        ShiftMaskArray = new Vector256<byte>[3];

        ShiftMaskArray[0] = Vector256.Create(0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255);

        for(int i = 0; i < 2; i++)
            ShiftMaskArray[i + 1] = Vector256.ShuffleNative(ShiftMaskArray[i], ShiftArray[i]);
    }
}
