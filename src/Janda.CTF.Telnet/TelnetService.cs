﻿using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
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
        private int _timeout;

        private const int DEFAULT_TIMEOUT = 100;
        private const string EOL_SEPARATOR = "\n";

            
        public TelnetService(ILogger<TelnetService> logger)
        {
            _logger = logger;
            _timeout = DEFAULT_TIMEOUT;
        }


        public void Connect()
        {
            if (_telnetAddress == null)
            {
                var stack = new StackTrace();                             
                var caller = stack.GetFrame(1).GetMethod();
                var attribute = caller.GetCustomAttribute<TelnetAttribute>() ?? throw new NotSupportedException($"Telnet attribute is missing in {caller.DeclaringType}.");    
                
                _logger.LogInformation("Using telnet {@options}", new { attribute.Host, attribute.Port });

                Connect(new TelnetAddress(attribute.Host, attribute.Port));
            }
        }


        public void Connect(TelnetAddress address)
        {
            _logger.LogTrace("Creating telnet connection");
            Disconnect();

            _telnetAddress = address ?? throw new ArgumentNullException("TelnetAddress", "Telnet address was not provided.");
            _logger.LogDebug("Connecting to {host}:{port}", address.Host, address.Port);
            _tcpClient = new TcpClient(address.Host, address.Port);
        }

        public void Connect(string host, int port)
        {
            Connect(new TelnetAddress(host, port));
        }

        public void Disconnect()
        {
            if (_tcpClient != null)
            {
                if (_tcpClient.Connected)
                {
                    _logger.LogTrace("Disconnecting from {host}:{port}", _telnetAddress.Host, _telnetAddress.Port);
                    _tcpClient.Close();    
                }
                
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


        public void WriteLine(string command)
        {
            Write(command + EOL_SEPARATOR);
        }

        public void Write(string command)
        {
            var buffer = Encoding.ASCII.GetBytes(command);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Sending {bytes} bytes: {@sb}", buffer.Length, command.Replace("\n", "\\n").Replace("\r", "\\r"));

            _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }


        public void WriteBytes(byte[] bytes)
        {         
            _logger.LogDebug("Sending {bytes} bytes: {dump}", bytes.Length, bytes.ToHexDump());
            _tcpClient.GetStream().Write(bytes, 0, bytes.Length);
        }
    }
}