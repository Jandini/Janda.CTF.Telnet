using Janda.Xunit.Logging;
using System.Net.Sockets;
using Xunit;
using Xunit.Abstractions;

namespace Janda.CTF.Telnet.Tests
{
    public class TelnetService_Must
    {
        private readonly XunitLogger<TelnetService> _logger;

        public TelnetService_Must(ITestOutputHelper output)
        {
            _logger = new XunitLogger<TelnetService>(output);
        }

        [Fact]
        public void IsDisconnected_AfterDisconnect()
        {
            using var telnet = new TelnetService(_logger);

            telnet.Connect("www.github.com", 80);
            Assert.True(telnet.IsConnected);
            telnet.Disconnect();
            Assert.False(telnet.IsConnected);
        }

        [Fact]
        public void Disconnect_AfterConnectWhenConnected()
        {
            using var telnet = new TelnetService(_logger);

            telnet.Connect("www.github.com", 80);
            Assert.True(telnet.IsConnected);
            telnet.Connect("www.github.com", 80);
            Assert.True(telnet.IsConnected);
            telnet.Disconnect();
            Assert.False(telnet.IsConnected);
        }


        [Fact]
        public void Reconnect_WhenConnectFromTcpClient()
        {
            using var telnet = new TelnetService(_logger);

            telnet.Connect(new TcpClient("www.github.com", 80));
            Assert.True(telnet.IsConnected);
            telnet.Connect();
            Assert.True(telnet.IsConnected);
            telnet.Disconnect();
            Assert.False(telnet.IsConnected);
        }
    }
}
