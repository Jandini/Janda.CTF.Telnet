using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Janda.CTF
{
    public class TelnetService : ITelnetService, IDisposable
    {
        private readonly ILogger<TelnetService> _logger;
        private TelnetAddress _telnetAddress;
        private TcpClient _tcpClient;
        private readonly int _timeout;
        public bool IsConnected => _tcpClient?.Connected ?? false;

        private const int DEFAULT_TIMEOUT = 100;
        private const string EOL_SEPARATOR = "\n";

        public TelnetService(ILogger<TelnetService> logger)
        {
            _logger = logger;
            _timeout = DEFAULT_TIMEOUT;            
        }



        private TelnetAddress GetTelnetAddress(MethodBase caller)
        {
            var attribute = caller.GetCustomAttribute<TelnetAttribute>();
                
            if (attribute is null) 
                throw new TelnetException($"Telnet attribute is missing in {caller.DeclaringType}.");

            _logger.LogInformation("Using telnet {@options}", new { attribute.Host, attribute.Port });

            return new TelnetAddress(attribute.Host, attribute.Port);
        }


        public void Connect()
        {
            Connect(_telnetAddress == null 
                ? GetTelnetAddress(new StackTrace().GetFrame(1).GetMethod()) 
                : _telnetAddress);
        }


        public void Connect(TelnetAddress address)
        {
            _logger.LogTrace("Creating telnet connection");

            Disconnect();

            _telnetAddress = address ?? throw new ArgumentNullException("TelnetAddress", "Telnet address was not provided.");
            _logger.LogDebug("Connecting to {host}:{port}", address.Host, address.Port);
            _tcpClient = new TcpClient(address.Host, address.Port);
        }


        public void Connect(TcpClient client)
        {            
            var ipep = client.Client.RemoteEndPoint as IPEndPoint;

            _tcpClient = client;
            _telnetAddress = new TelnetAddress(ipep.Address.ToString(), ipep.Port);
        }

        public void Connect(string host, int port)
        {
            Connect(new TelnetAddress(host, port));
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _logger.LogTrace("Disconnecting from {host}:{port}", _telnetAddress.Host, _telnetAddress.Port);
                _tcpClient.Close();
                _tcpClient.Dispose();
                _tcpClient = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void Parse(StringBuilder sb)
        {
            while (_tcpClient.Available > 0)
            {
                int input = _tcpClient.GetStream().ReadByte();

                switch (input)
                {
                    case -1:
                        break;

                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = _tcpClient.GetStream().ReadByte();      
                        
                        if (inputverb == -1) 
                            break;

                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                // literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;

                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = _tcpClient.GetStream().ReadByte();
                                if (inputoption == -1) 
                                    break;

                                _tcpClient.GetStream().WriteByte((byte)Verbs.IAC);
                                
                                if (inputoption == (int)Options.SGA)
                                    _tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    _tcpClient.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);

                                _tcpClient.GetStream().WriteByte((byte)inputoption);
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }



        public string[] ReadLines()
        {
            return Read().Split("\n");
        }


        public string Read()
        {
            var builder = new StringBuilder();
            
            do
            {
                var available = _tcpClient.Available;
                
                if (available > 0)
                    _logger.LogTrace($"Receiving {{bytes}} {"byte".ToPlural(available)}", available);
                else
                    _logger.LogDebug("Waiting for bytes");

                Parse(builder);
                Thread.Sleep(_timeout);

            } while (_tcpClient.Available > 0);

            var result = builder.ToString();

            if (string.IsNullOrEmpty(result))
                _logger.LogDebug("No bytes received");
            else
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    var chars = result.Count(x => x != '\n' && x != '\r');
                    var lf = result.Count(x => x == (char)0xA);
                    var cr = result.Count(x => x == (char)0xD);

                    var parameters = cr > 0 ? new object[] { chars, lf, cr, result } : new object[] { chars, lf, result };
                    var message = $"Received {{chars}} {"character".ToPlural(chars)} in {{lines}} {"line feed".ToPlural(lf)}";

                    if (cr > 0)
                        message += $" with {{cr}} {"carriage return ".ToPlural(cr)}";

                    message += "\n{payload}";

                    _logger.LogTrace(message, parameters);
                }
            }

            return result;
        }


        public void WriteLine(string value)
        {
            Write(value + EOL_SEPARATOR);
        }

        public void Write(string value)
        {
            var buffer = Encoding.ASCII.GetBytes(value);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Sending {bytes} bytes: {@sb}", buffer.Length, value.Replace("\n", "\\n").Replace("\r", "\\r"));

            _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }


        public void WriteBytes(byte[] bytes)
        {         
            _logger.LogDebug("Sending {bytes} bytes: {dump}", bytes.Length, bytes.ToHexDump());
            _tcpClient.GetStream().Write(bytes, 0, bytes.Length);
        }
    }
}
