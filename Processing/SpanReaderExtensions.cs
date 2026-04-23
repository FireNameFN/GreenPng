using System;
using System.Buffers.Binary;
using GreenBuf;

namespace GreenPng.Processing;

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

            type = (ChunkType)reader.GetInt32();

            chunk = reader.Get(length);

            reader.Advance(4);

            return true;
        }

        public int GetInt32() {
            ReadOnlySpan<byte> span = reader.Get(4);

            return BinaryPrimitives.ReadInt32BigEndian(span);
        }
    }
}
