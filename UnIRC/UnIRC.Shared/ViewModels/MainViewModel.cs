﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Views;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Messages;
using UnIRC.Shared.Models;

namespace UnIRC.ViewModels
{
    public class MainViewModel : ViewModelBaseExtended
    {
        private INavigationService _navigationService;


        public ObservableCollection<MenuItem> UpperFixedMenu
        {
            get { return _upperFixedMenu; }
            set { Set(ref _upperFixedMenu, value); }
        }
        private ObservableCollection<MenuItem> _upperFixedMenu
            = new ObservableCollection<MenuItem>
            {
                new MenuItem("\uE710", "Connect", "Networks")
            };

        public int UpperFixedMenuSelectedIndex
        {
            get { return _upperFixedMenuSelectedIndex; }
            set { Set(ref _upperFixedMenuSelectedIndex, value); }
        }
        private int _upperFixedMenuSelectedIndex;

        public ObservableCollection<MenuItem> LowerFixedMenu
        {
            get { return _lowerFixedMenu; }
            set { Set(ref _lowerFixedMenu, value); }
        }
        private ObservableCollection<MenuItem> _lowerFixedMenu
            = new ObservableCollection<MenuItem>
            {
                new MenuItem("\uE81C", "Debug Log", "Log")
            };

        public ObservableCollection<ConnectionViewModel> Connections
        {
            get { return _connections; }
            set { Set(ref _connections, value); }
        }
        private ObservableCollection<ConnectionViewModel> _connections
            = new ObservableCollection<ConnectionViewModel>();

        public ConnectionViewModel SelectedConnection
        {
            get { return _selectedConnection; }
            set { Set(ref _selectedConnection, value); }
        }
        private ConnectionViewModel _selectedConnection;

        public ConnectionViewModel MenuSelectedConnection
        {
            get { return _menuSelectedConnection; }
            set { Set(ref _menuSelectedConnection, value); }
        }
        private ConnectionViewModel _menuSelectedConnection;

        public ObservableCollection<ChannelViewModel> ConnectionChannels
        {
            get { return _connectionChannels; }
            set { Set(ref _connectionChannels, value); }
        }
        private ObservableCollection<ChannelViewModel> _connectionChannels
            = new ObservableCollection<ChannelViewModel>();





        public ObservableCollection<ErrorMessage> Errors
        {
            get { return _errors; }
            set { Set(ref _errors, value); }
        }
        private ObservableCollection<ErrorMessage> _errors
            = new ObservableCollection<ErrorMessage>();

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set { Set(ref _messages, value); }
        }
        private ObservableCollection<Message> _messages
            = new ObservableCollection<Message>();

        private readonly IConnectionEndpoint _directServerEndpoint = new DirectServerConnectionEndpoint();
        
        public ICommand ClearErrorsCommand { get; set; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            
            this.OnChanged(x => x.SelectedConnection).Do(
                () =>
                {
                    if (SelectedConnection != null)
                    {
                        MenuSelectedConnection = SelectedConnection;
                        ConnectionChannels = MenuSelectedConnection.Channels;
                    }
                    else if (!Connections.Contains(MenuSelectedConnection))
                        ConnectionChannels = null;
                });

            Register<ErrorMessage>(m => Errors.Add(m));
            Register<Message>(true, m => Messages.Add(m));
            Register<ConnectMessage>(AddConnection);
        }

        private void AddConnection(ConnectMessage m)
        {
            var connection = new ConnectionViewModel(
                m.Network, m.Server, m.UserInfo, _directServerEndpoint);
            Connections.Add(connection);
        }
    }
}
