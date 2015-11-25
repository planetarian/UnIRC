using System;
using System.IO;
using System.Text.RegularExpressions;

namespace UnIRC.IrcEvents
{
    public class IrcMessage
    {
        public string RawMessage { get; set; }
        public string Prefix { get; set; }
        public string Command { get; set; }
        public string[] Parameters { get; set; }
        public string ParameterString { get; set; }
        public string Trailing { get; set; }


        private const string _messageRegexPatternOld =
            @"^(?::(?<prefix>\S+) )?(?<command>\S+)(?<innerparams>( +[^ ]+)*?( +:(?<outerparams>.*))?)?$";

        private const string _messageRegexPattern =
            @"^(?::(?<prefix>\S+) )?(?<command>\S+)(?: (?!:)(?<params>.+?))?( :(?<trail>.+))?$";

        internal IrcMessage(string rawData)//, string prefix, string command, string[] parameters, string trailing)
        {
            RawMessage = rawData;
            Match match = Regex.Match(rawData, _messageRegexPattern, RegexOptions.Compiled);
            if (!match.Success)
                throw new InvalidDataException("IRCMessage data invalid.");

            Prefix = match.Groups["prefix"].Value;
            Command = match.Groups["command"].Value;
            ParameterString = match.Groups["params"].Value;
            Parameters = ParameterString
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Trailing = match.Groups["trail"].Value;
        }
    }
}
