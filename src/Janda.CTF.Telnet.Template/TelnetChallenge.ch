using Microsoft.Extensions.Logging;

namespace {{Namespace}}
{
    public class {{ChallengeName}} : IChallenge
    {
        private readonly ILogger<{{ChallengeName}}> _logger;
        private readonly ITelnetService _telnet;

        public {{ChallengeName}}(ILogger<{{ChallengeName}}> logger, ITelnetService telnet)
        {
            _logger = logger;
            _telnet = telnet;
        }

        [Telnet(Host = "{{Host}}", Port = {{Port}})]
        public void Run()
        {
            _telnet.Connect();
            _telnet.Read();
        }
    }
}
