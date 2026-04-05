using System;
using System.Collections.Generic;
using System.IO;

namespace GreenPng.Processing;

public sealed unsafe class SegmentStream(byte* buffer) : Stream {
    readonly byte* buffer = buffer;

    readonly List<Region> regions = [];

    int regionIndex;

    int regionOffset;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public void Add(int offset, int length) {
        regions.Add(new(offset, length));
    }

    public override int Read(byte[] buffer, int offset, int count) {
        if(regionIndex >= regions.Count)
            return 0;

        Region region = regions[regionIndex];

        Span<byte> span = new(this.buffer + region.Offset + regionOffset, Math.Min(region.Length - regionOffset, count));

        span.CopyTo(buffer.AsSpan(offset));

        regionOffset += span.Length;

        if(regionOffset >= region.Length) {
            regionIndex++;

            regionOffset = 0;
        }

        return span.Length;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        throw new NotSupportedException();
    }

    public override void SetLength(long value) {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count) {
        throw new NotSupportedException();
    }

    public override void Flush() {
        throw new NotSupportedException();
    }

    readonly struct Region(int offset, int length) {
        public readonly int Offset = offset;

        public readonly int Length = length;
    }
}
