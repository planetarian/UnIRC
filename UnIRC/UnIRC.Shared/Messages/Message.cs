using System;
using UnIRC.Shared.Helpers;

namespace UnIRC.Shared.Messages
{
    public class Message
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public Message(string name, string description)
        {
            Date = Util.Now;

            Name = name;
            Description = description;
        }
    }
}
