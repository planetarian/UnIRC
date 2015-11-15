using GalaSoft.MvvmLight.Views;
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
        INavigationService _navigationService;

        // Network:

        private ObservableCollection<NetworkViewModel> _networks
            = new ObservableCollection<NetworkViewModel>();
        public ObservableCollection<NetworkViewModel> Networks
        {
            get { return _networks; }
            set { Set(nameof(Networks), ref _networks, value); }
        }

        private NetworkViewModel _selectedNetwork;
        public NetworkViewModel SelectedNetwork
        {
            get { return _selectedNetwork; }
            set { Set(nameof(SelectedNetwork), ref _selectedNetwork, value); }
        }

        private string _newNetworkName;
        public string NewNetworkName
        {
            get { return _newNetworkName; }
            set { Set(nameof(NewNetworkName), ref _newNetworkName, value); }
        }

        private bool _addingNewNetwork;
        public bool AddingNewNetwork
        {
            get { return _addingNewNetwork; }
            set { Set(nameof(AddingNewNetwork), ref _addingNewNetwork, value); }
        }
        

        // Network CRUD:
        
        public ICommand CreateNewNetworkCommand { get; set; }
        public ICommand SaveNewNetworkCommand { get; set; }
        public ICommand EditNetworkCommand { get; set; }
        public ICommand DeleteNetworkCommand { get; set; }


        public NetworksViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            CreateNewNetworkCommand = GetCommand(CreateNewNetwork);
            SaveNewNetworkCommand = GetCommand(SaveNewNetwork, () => !NewNetworkName.IsNullOrEmpty(), () => NewNetworkName);
            EditNetworkCommand = GetCommand(EditSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
            DeleteNetworkCommand = GetCommand(DeleteSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
        }

        // Network CRUD:
        
        private void CreateNewNetwork()
        {
            NewNetworkName = "";
            AddingNewNetwork = true;
        }

        private void SaveNewNetwork()
        {
            AddingNewNetwork = false;
            var network = new Network() { Name = NewNetworkName };
            var networkViewModel = new NetworkViewModel { Network = network };
            Networks.Add(networkViewModel);
            SelectedNetwork = networkViewModel;
            NewNetworkName = null;
        }

        private void EditSelectedNetwork()
        {
            throw new NotImplementedException();
        }

        private void DeleteSelectedNetwork()
        {
            NetworkViewModel selected = SelectedNetwork;
            SelectedNetwork = null;
            Networks.Remove(selected);
        }
    }
}
