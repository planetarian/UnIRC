using System;
using UnIRC.Shared.Helpers;

namespace UnIRC.Shared.Messages
{
    public class Message
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public DateTime Date { get; protected set; }

        public Message(string name, string description)
        {
            Date = Util.Now;

            Name = name;
            Description = description;
        }
    }
}
