using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class UdpBroadcast_Tests
    {
        [TestMethod]
        public async Task BroadcastAsync_Tests()
        {
            if (!Debugger.IsAttached) return;// GitHub doesn't seem to support networking (or UDP broadcasts on the loopback adapter?) during tests
            Broadcaster bc = new();
            await using (bc)
            {
                byte[] data = new byte[] { 1, 2, 3 };
                byte[]?[] responses = await bc.BroadcastAsync(data, TimeSpan.FromMilliseconds(500));
                Assert.AreEqual(1, responses.Length);
                foreach (byte[]? response in responses)
                    Assert.IsTrue(response is null || response.SequenceEqual(data));
            }
        }

        [TestMethod]
        public async Task ListenerAsync_Tests()
        {
            if (!Debugger.IsAttached) return;// GitHub doesn't seem to support networking (or UDP broadcasts on the loopback adapter?) during tests
            Broadcaster bc = new();
            await using (bc)
            {
                await bc.StartAsync();
                await Task.Delay(500);
                using UdpClient client = new(new IPEndPoint(IPAddress.Loopback, 23456))
                {
                    EnableBroadcast = true
                };
                client.Client.DontFragment = true;
                byte[] data = new byte[] { 1, 2, 3 };
                await client.SendAsync(data, new IPEndPoint(IPAddress.Loopback, 12345));
                await Task.Delay(500);
                Assert.AreEqual(1, bc.PacketsReceived);
                UdpReceiveResult result = default;
                async Task receive()
                {
                    result = await client.ReceiveAsync();
                }
                Task task = receive();
                await bc.BroadcastAsync(data, 23456);
                await task.WaitAsync(TimeSpan.FromMilliseconds(1000));
                Assert.IsTrue(result.Buffer.SequenceEqual(data));
                await bc.StopAsync();
            }
        }

        private sealed class Broadcaster : UdpBroadcast<byte[]>
        {
            public Broadcaster() : base(new(IPAddress.Loopback, 12345)) { }

            public int PacketsReceived { get; private set; }

            protected override Task HandleReceivedAsync(UdpReceiveResult packet, CancellationToken cancellationToken)
            {
                PacketsReceived++;
                return Task.CompletedTask;
            }

            protected override byte[]? HandleResponse(in UdpReceiveResult response) => response.Buffer;
        }
    }
}
