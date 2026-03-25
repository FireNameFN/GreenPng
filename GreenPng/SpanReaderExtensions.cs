using System;
using System.Buffers.Binary;
using System.IO.Hashing;
using GreenBuf;

namespace GreenPng;

public static class SpanReaderExtensions {
    extension(ref SpanReader reader) {
        public bool TryGetChunk(out ChunkType type, out ReadOnlySpan<byte> chunk) {
            type = default;

            chunk = default;

            if(!reader.Check(12))
                return false;

            int length = reader.GetInt32();

            if(!reader.Check(length + 8))
                return false;

            ReadOnlySpan<byte> data = reader.Get(length + 4);

            uint hash = Crc32.HashToUInt32(data);

            uint hashRef = (uint)reader.GetInt32();

            if(hash != hashRef)
                return false;

            type = (ChunkType)BinaryPrimitives.ReadInt32BigEndian(data[..4]);

            chunk = data[4..];

            return true;
        }

        public int GetInt32() {
            ReadOnlySpan<byte> span = reader.Get(4);

            return BinaryPrimitives.ReadInt32BigEndian(span);
        }
    }
}
