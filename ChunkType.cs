namespace GreenPng;

public enum ChunkType {
    IHDR = 0x49484452,
    IDAT = 0x49444154,
    IEND = 0x49454E44,
    PLTE = 0x504C5445,
    tRNS = 0x74524E53
}
