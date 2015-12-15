using System;
using System.Collections.Generic;
using System.Text;
using UnIRC.Models;

namespace UnIRC.Shared.Models
{
    public class ConnectionModel
    {
        public int ConnectionId { get; set; }
        public Network Network { get; set; }
        public Server Server { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public UserInfo UserInfo { get; set; }
        public string Nick { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string DefaultNick { get; set; }
        public string BackupNick { get; set; }
        public string DisplayNick { get; set; }
        public bool IsConnected { get; set; }
        public int UnreadMessagesCount { get; set; }
        public int LastSuccessfulPort { get; set; }
        public List<string> Messages { get; set; }
        public List<string> UnreadMessages { get; set; }
        public List<string> SentMessages { get; set; }
        public List<string> Motd { get; set; }
    }
}
