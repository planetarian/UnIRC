using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ConnectionViewModel : ViewModelBaseExtended
    {
        private static int _nextConnectionId = 1;

        public int ConnectionId
        {
            get { return _connectionId; }
            set { Set(ref _connectionId, value); }
        }
        private int _connectionId;

        public Network Network
        {
            get { return _network; }
            set { Set(ref _network, value); }
        }
        private Network _network;

        public Server Server
        {
            get { return _server; }
            set { Set(ref _server, value); }
        }
        private Server _server;

        public string DisplayName
        {
            get { return _displayName; }
            set { Set(ref _displayName, value); }
        }
        private string _displayName;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }
        private bool _isConnected;

        public int UnreadMessages
        {
            get { return _unreadMessages; }
            set { Set(ref _unreadMessages, value); }
        }
        private int _unreadMessages;

        public ICommand ReconnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        public ConnectionViewModel(Network network, Server server)
        {
            ConnectionId = _nextConnectionId++;

            this.OnChanged(x => x.Network).Do(() => DisplayName = Network?.Name);

            Network = network;
            Server = server;
        }
    }
}
