using BenchmarkDotNet.Attributes;
using Microsoft.IO;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Pool_Tests
    {
        private static readonly RecyclableMemoryStreamManager MemManager;
        private static readonly byte[] TestData;
        private static readonly byte[] TestBuffer;
        private static readonly MemoryStream TestNetStream;
        private static readonly RecyclableMemoryStream TestRecyclableStream;
        private static readonly FreezableArrayPoolStream TestArrayStream;
        private static readonly FreezableMemoryPoolStream TestMemoryStream;

        static Pool_Tests()
        {
            MemManager = new();
            TestData = RandomNumberGenerator.GetBytes(200000);
            TestBuffer = new byte[200000];
            TestNetStream = new(TestData);
            Contract.Assert(!TestNetStream.CanWrite);
            TestRecyclableStream = new(MemManager);
            TestRecyclableStream.Write(TestData);
            TestArrayStream = new(TestData);
            TestArrayStream.Freeze();
            Contract.Assert(!TestArrayStream.CanWrite);
            TestMemoryStream = new(new MemoryOwner<byte>(TestData), returnData: true);
            Contract.Assert(!TestMemoryStream.CanWrite);
        }

        [Benchmark]
        public void NetWrite()
        {
            using MemoryStream ms = new();
            ms.WriteByte(0);
            ms.Write(TestData);
            ms.WriteByte(0);
        }

        [Benchmark]
        public void RecyclableWrite()
        {
            using RecyclableMemoryStream ms = new(MemManager);
            ms.WriteByte(0);
            ms.Write(TestData);
            ms.WriteByte(0);
        }

        [Benchmark]
        public void ArrayWrite()
        {
            using ArrayPoolStream ms = new();
            ms.WriteByte(0);
            ms.Write(TestData);
            ms.WriteByte(0);
        }

        [Benchmark]
        public void MemoryWrite()
        {
            using MemoryPoolStream ms = new();
            ms.WriteByte(0);
            ms.Write(TestData);
            ms.WriteByte(0);
        }

        [Benchmark]
        public void NetWriteRead()
        {
            using MemoryStream ms = new();
            ms.Write(TestData);
            ms.Position = 0;
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void RecyclableWriteRead()
        {
            using RecyclableMemoryStream ms = new(MemManager, tag: null);
            ms.Write(TestData);
            ms.Position = 0;
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void ArrayWriteRead()
        {
            using ArrayPoolStream ms = new(TestData);
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void MemoryWriteRead()
        {
            using MemoryPoolStream ms = new();
            ms.Write(TestData);
            ms.Position = 0;
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void NetRead()
        {
            using MemoryStream ms = new(TestData);
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void MemoryRead()
        {
            using MemoryPoolStream ms = new(new MemoryOwner<byte>(TestData), returnData: true);
            ms.ReadByte();
            ms.Read(TestBuffer);
            ms.ReadByte();
        }

        [Benchmark]
        public void NetReadOnly()
        {
            TestNetStream.Position = 0;
            TestNetStream.Read(TestBuffer);
        }

        [Benchmark]
        public void RecyclableReadOnly()
        {
            TestRecyclableStream.Position = 0;
            TestRecyclableStream.Read(TestBuffer);
        }

        [Benchmark]
        public void ArrayReadOnly()
        {
            TestArrayStream.Position = 0;
            TestArrayStream.Read(TestBuffer);
        }

        [Benchmark]
        public void MemoryReadOnly()
        {
            TestMemoryStream.Position = 0;
            TestMemoryStream.Read(TestBuffer);
        }

        [Benchmark]
        public byte[] NetToArray()
        {
            using MemoryStream ms = new();
            ms.Write(TestData);
            return ms.ToArray();
        }

        [Benchmark]
        public byte[] RecyclableToArray()
        {
            using RecyclableMemoryStream ms = new(MemManager);
            ms.Write(TestData);
            return ms.GetReadOnlySequence().ToArray();
        }

        [Benchmark]
        public byte[] ArrayToArray()
        {
            using ArrayPoolStream ms = new();
            ms.Write(TestData);
            return ms.ToArray();
        }

        [Benchmark]
        public byte[] MemoryToArray()
        {
            using MemoryPoolStream ms = new();
            ms.Write(TestData);
            return ms.ToArray();
        }

        [Benchmark]
        public void NetSetLength()
        {
            using MemoryStream ms = new();
            ms.SetLength(1);
            ms.SetLength(ms.Capacity + 1);
            ms.SetLength(1);
        }

        [Benchmark]
        public void RecyclableSetLength()
        {
            using RecyclableMemoryStream ms = new(MemManager);
            ms.SetLength(1);
            ms.SetLength(ms.Capacity + 1);
            ms.SetLength(1);
        }

        [Benchmark]
        public void ArraySetLength()
        {
            using ArrayPoolStream ms = new();
            ms.SetLength(1);
            ms.SetLength(ms.BufferLength + 1);
            ms.SetLength(1);
        }

        [Benchmark]
        public void MemorySetLength()
        {
            using MemoryPoolStream ms = new();
            ms.SetLength(1);
            ms.SetLength(ms.BufferLength + 1);
            ms.SetLength(1);
        }
    }
}
