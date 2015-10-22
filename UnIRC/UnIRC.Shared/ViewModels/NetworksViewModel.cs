using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UnIRC.Shared.Helpers;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class NetworksViewModel : ViewModelBaseExtended
    {
        // Network:

        private ObservableCollection<NetworkViewModel> _networks;
        public ObservableCollection<NetworkViewModel> Networks
        {
            get { return _networks; }
            set { Set(nameof(Networks), ref _networks, value); }
        }

        private Network _selectedNetwork;
        public Network SelectedNetwork
        {
            get { return _selectedNetwork; }
            set { Set(nameof(SelectedNetwork), ref _selectedNetwork, value); }
        }

        // Server:

        private ObservableCollection<Server> _selectedNetworkServers;
        public ObservableCollection<Server> SelectedNetworkServers
        {
            get { return _selectedNetworkServers; }
            set { Set(nameof(SelectedNetworkServers), ref _selectedNetworkServers, value); }
        }

        private Server _selectedServer;
        public Server SelectedServer
        {
            get { return _selectedServer; }
            set { Set(nameof(SelectedServer), ref _selectedServer, value); }
        }

        // Network CRUD:
        
        public ICommand AddNetworkCommand { get; set; }
        public ICommand EditNetworkCommand { get; set; }
        public ICommand DeleteNetworkCommand { get; set; }


        public NetworksViewModel()
        {
            AddNetworkCommand = GetCommand(AddNewNetwork);
            EditNetworkCommand = GetCommand(EditSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
            DeleteNetworkCommand = GetCommand(DeleteSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);


            this.OnChanged(x => x.SelectedNetwork)
                .Do(() => SelectedNetworkServers = SelectedNetwork.Servers.ToObservable());

        }

        private void AddNewNetwork()
        {
            throw new NotImplementedException();
        }

        private void EditSelectedNetwork()
        {
            throw new NotImplementedException();
        }

        private void DeleteSelectedNetwork()
        {
            throw new NotImplementedException();
        }
    }
}
