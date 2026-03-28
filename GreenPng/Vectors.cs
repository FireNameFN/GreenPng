using System.Runtime.Intrinsics;

namespace GreenPng;

public static class Vectors {
    public static readonly Vector256<byte> Shuffle256 = Vector256.Create((byte)2, 1, 0, 3, 5, 4, 3, 7, 8, 7, 6, 11, 11, 10, 9, 15, 14, 13, 12, 19, 17, 16, 15, 23, 20, 19, 18, 27, 23, 22, 21, 31);

    public static readonly Vector256<byte> ShuffleAlpha256 = Vector256.Create((byte)2, 1, 0, 3, 6, 5, 4, 7, 10, 9, 8, 11, 14, 13, 12, 15, 18, 17, 16, 19, 22, 21, 20, 23, 26, 25, 24, 27, 30, 29, 28, 31);

    public static readonly Vector256<byte> ShuffleMono256 = Vector256.Create((byte)0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7);

    public static readonly Vector256<byte> MaskMono256 = Vector256.Create(255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0);

    public static readonly Vector256<byte> MaskAlpha256 = Vector256.Create(0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255);

    public static readonly Vector256<byte> LastShift256 = Vector256.Create((byte)28, 29, 30, 31, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31);

    public static readonly Vector256<byte> LastShiftMask256 = Vector256.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static readonly Vector256<byte>[] ShiftArray256;

    public static readonly Vector256<byte>[] ShiftMaskArray256;

    public static readonly Vector128<byte> PixelMask128 = Vector128.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static readonly Vector128<byte> Shift128 = Vector128.Create((byte)12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);

    public static readonly Vector128<byte>[] MaskArray128;

    static Vectors() {
        ShiftArray256 = new Vector256<byte>[3];
        
        ShiftArray256[0] = Vector256.Create((byte)0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27);

        for(int i = 0; i < 2; i++)
            ShiftArray256[i + 1] = Vector256.ShuffleNative(ShiftArray256[i], ShiftArray256[i]);

        ShiftMaskArray256 = new Vector256<byte>[3];

        ShiftMaskArray256[0] = Vector256.Create(0, 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255);

        for(int i = 0; i < 2; i++)
            ShiftMaskArray256[i + 1] = Vector256.ShuffleNative(ShiftMaskArray256[i], ShiftArray256[i]);

        MaskArray128 = new Vector128<byte>[4];

        MaskArray128[0] = Vector128.Create(255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        for(int i = 0; i < 3; i++)
            MaskArray128[i + 1] = Vector128.ShuffleNative(MaskArray128[i], Shift128);
    }
}
