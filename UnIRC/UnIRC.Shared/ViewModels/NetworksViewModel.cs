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

        // Network editing properties:

        private string _newNetworkName;
        public string NewNetworkName
        {
            get { return _newNetworkName; }
            set { Set(nameof(NewNetworkName), ref _newNetworkName, value); }
        }

        // State:

        private bool _isAddingNewNetwork;
        public bool IsAddingNewNetwork
        {
            get { return _isAddingNewNetwork; }
            set { Set(nameof(IsAddingNewNetwork), ref _isAddingNewNetwork, value); }
        }

        private bool _isEditingNetwork;
        public bool IsEditingNetwork
        {
            get { return _isEditingNetwork; }
            set { Set(nameof(IsEditingNetwork), ref _isEditingNetwork, value); }
        }

        // Network CRUD:
        
        public ICommand CreateNewNetworkCommand { get; set; }
        public ICommand EditNetworkCommand { get; set; }
        public ICommand SaveNetworkCommand { get; set; }
        public ICommand CancelNetworkOperationCommand { get; set; }
        public ICommand DeleteNetworkCommand { get; set; }


        public NetworksViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            CreateNewNetworkCommand = GetCommand(CreateNewNetwork);
            EditNetworkCommand = GetCommand(EditNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
            SaveNetworkCommand = GetCommand(SaveNetwork, () => !NewNetworkName.IsNullOrEmpty(), () => NewNetworkName);
            CancelNetworkOperationCommand = GetCommand(CancelNetworkOperation);
            DeleteNetworkCommand = GetCommand(DeleteSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
        }

        // Network CRUD:
        
        private void CreateNewNetwork()
        {
            ClearNetworkForm();
            IsAddingNewNetwork = true;
            IsEditingNetwork = true;
        }

        private void EditNetwork()
        {
            GetNetworkProperties(SelectedNetwork);
            IsEditingNetwork = true;
        }

        private void SaveNetwork()
        {
            if (IsEditingNetwork)
            {
                NetworkViewModel editedNetwork = SelectedNetwork;
                SelectedNetwork = null;
                if (IsAddingNewNetwork)
                {
                    editedNetwork = new NetworkViewModel();
                    Networks.Add(editedNetwork);
                }
                ApplyNewNetworkProperties(editedNetwork);
                SelectedNetwork = editedNetwork;
            }
            CancelNetworkOperation();
        }
        
        private void CancelNetworkOperation()
        {
            IsAddingNewNetwork = false;
            IsEditingNetwork = false;
            ClearNetworkForm();
        }

        private void GetNetworkProperties(NetworkViewModel sourceNetwork)
        {
            NewNetworkName = sourceNetwork?.Name;
        }

        private void ApplyNewNetworkProperties(NetworkViewModel targetNetwork)
        {
            targetNetwork.Name = NewNetworkName;
        }

        private void ClearNetworkForm()
        {
            NewNetworkName = "";
        }

        private void DeleteSelectedNetwork()
        {
            Networks.Remove(SelectedNetwork);
            SelectedNetwork = null;
        }
    }
}
