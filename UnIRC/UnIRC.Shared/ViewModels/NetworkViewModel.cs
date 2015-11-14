using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class NetworkViewModel : ViewModelBaseExtended
    {
        private Network _network;
        public Network Network
        {
            get { return _network; }
            set { Set(nameof(Network), ref _network, value); }
        }

        // Edit Network:

        private string _networkName;
        public string NetworkName
        {
            get { return _networkName; }
            set { Set(nameof(NetworkName), ref _networkName, value); }
        }

        // Edit Servers:

        private ObservableCollection<Server> _servers;
        public ObservableCollection<Server> Servers
        {
            get { return _servers; }
            set { Set(nameof(Servers), ref _servers, value); }
        }

        private Server _selectedServer;
        public Server SelectedServer
        {
            get { return _selectedServer; }
            set { Set(nameof(SelectedServer), ref _selectedServer, value); }
        }

        public ICommand AddServerCommand { get; set; }
        public ICommand EditServerCommand { get; set; }
        public ICommand DeleteServerCommand { get; set; }


        public NetworkViewModel()
        {
            AddServerCommand = GetCommand(AddNewServer);
            EditServerCommand = GetCommand(EditSelectedServer, () => SelectedServer != null, () => SelectedServer);
            DeleteServerCommand = GetCommand(DeleteSelectedServer, () => SelectedServer != null, () => SelectedServer);
        }

        // Network CRUD:

        private void AddNewServer()
        {
            throw new NotImplementedException();
        }

        private void EditSelectedServer()
        {
            throw new NotImplementedException();
        }

        private void DeleteSelectedServer()
        {
            throw new NotImplementedException();
        }

    }
}
