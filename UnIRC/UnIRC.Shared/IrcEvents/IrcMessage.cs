namespace UnIRC.IrcEvents
{
    public class IrcMessage
    {
        public string RawMessage { get; private set; }
        public string Prefix { get; private set; }
        public string Command { get; private set; }
        public string[] Parameters { get; private set; }
        public string Trailing { get; private set; }


        internal IrcMessage(string rawMessage, string prefix, string command, string[] parameters, string trailing)
        {
            RawMessage = rawMessage;
            Prefix = prefix;
            Command = command;
            Parameters = parameters;
            Trailing = trailing;
        }
    }
}
