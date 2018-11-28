﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ZWave4Net.Channel.Protocol.Frames
{
    public static partial class Extentions
    {
        public static byte CalculateChecksum(this IEnumerable<byte> values)
        {
            return values.Aggregate((byte)0xFF, (total, next) => total ^= next);
        }

        public static Task WriteHeader(this IDuplexStream stream, FrameHeader header, CancellationToken cancellation)
        {
            return stream.Write(new[] { (byte)header }, cancellation);
        }

        public static async Task<byte> ReadByte(this IDuplexStream stream, CancellationToken cancellation)
        {
            return (await stream.Read(1, cancellation)).Single();
        }

        public static async Task<FrameHeader> ReadHeader(this IDuplexStream stream, CancellationToken cancellation)
        {
            return (FrameHeader)(await stream.Read(1, cancellation)).Single();
        }
    }
}