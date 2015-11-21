﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Views;
using UnIRC.Models;
using UnIRC.Shared.Messages;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
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
        
        public ICommand ClearErrorsCommand { get; set; }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            Register<ErrorMessage>(m => Errors.Add(m));
            Register<Message>(true, m => Messages.Add(m));
            Register<ConnectMessage>(AddConnection);
        }

        private void AddConnection(ConnectMessage m)
        {
            var connection = new ConnectionViewModel(m.Network, m.Server);
            Connections.Add(connection);
        }
    }
}
