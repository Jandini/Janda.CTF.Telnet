using CommandLine;

namespace Janda.CTF
{
    [Verb("add-telnet", isDefault: false, HelpText = "Add telnet challenge.")]
    public class TelnetChallengeTemplateOptions : ChallengeTemplateOptions
    {
        [Option("template", Default = "TelnetChallenge", HelpText = "Template name.")]
        public override string TemplateName { get; set; } = "TelnetChallenge";

        [Option("host", HelpText = "Telnet host")]
        public string Host { get; set; }

        [Option("port", HelpText = "Telnet port")]
        public int? Port { get; set; }
    }
}
