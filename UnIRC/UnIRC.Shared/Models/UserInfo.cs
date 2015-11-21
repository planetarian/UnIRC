using System;
using System.Collections.Generic;
using System.Text;

namespace UnIRC.Shared.Models
{
    public class UserInfo
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Nick { get; set; }
        public string BackupNick { get; set; }
    }
}
