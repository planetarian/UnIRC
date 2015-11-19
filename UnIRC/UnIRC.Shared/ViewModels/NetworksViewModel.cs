
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.Views;
using Newtonsoft.Json;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Messages;
using UnIRC.ViewModels;
#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace UnIRC.Shared.ViewModels
{
    public class NetworksViewModel : ViewModelBaseExtended
    {
        private INavigationService _navigationService;

        // Network:

        public ObservableCollection<NetworkViewModel> Networks
        {
            get { return _networks; }
            set { Set(ref _networks, value); }
        }
        private ObservableCollection<NetworkViewModel> _networks
            = new ObservableCollection<NetworkViewModel>();

        public NetworkViewModel SelectedNetwork
        {
            get { return _selectedNetwork; }
            set { Set(ref _selectedNetwork, value); }
        }
        private NetworkViewModel _selectedNetwork;

        public Settings Settings
        {
            get { return _settings; }
            set { Set(ref _settings, value); }
        }
        private Settings _settings;

        // Network editing properties:

        public string NewNetworkName
        {
            get { return _newNetworkName; }
            set { Set(ref _newNetworkName, value); }
        }
        private string _newNetworkName;

        // State:

        public bool IsAddingNewNetwork
        {
            get { return _isAddingNewNetwork; }
            set { Set(ref _isAddingNewNetwork, value); }
        }
        private bool _isAddingNewNetwork;

        public bool IsEditingNetwork
        {
            get { return _isEditingNetwork; }
            set { Set(ref _isEditingNetwork, value); }
        }
        private bool _isEditingNetwork;

        public bool IsDeletingNetwork
        {
            get { return _isDeletingNetwork; }
            set { Set(ref _isDeletingNetwork, value); }
        }
        private bool _isDeletingNetwork;


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

            Register<NetworksModifiedMessage>(SaveNetworks);

            this.OnChanged(x => x.SelectedNetwork)
                .Do(() =>
                {
                    Send(new NetworksModifiedMessage());
                    if (SelectedNetwork != null)
                        SelectedNetwork.SelectedServer = SelectedNetwork.Servers?.FirstOrDefault();
                });

#if WINDOWS_UWP
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            var networksJson = roamingSettings.Values["Networks"] as string;
            var selectedNetworkJson = roamingSettings.Values["SelectedNetwork"] as string;
            var selectedServerJson = roamingSettings.Values["SelectedServer"] as string;

            if (networksJson == null)
                return;
            var networks = JsonConvert.DeserializeObject<List<Network>>(networksJson);
            Networks = networks.Select(n => new NetworkViewModel(n)).ToObservable();

            if (selectedNetworkJson == null)
                return;
            var selectedNetwork = JsonConvert.DeserializeObject<Network>(selectedNetworkJson);
            SelectedNetwork = Networks.FirstOrDefault(n => n.Network?.Name == selectedNetwork.Name);

            if (selectedServerJson == null)
                return;
            var selectedServer = JsonConvert.DeserializeObject<Server>(selectedServerJson);
            SelectedNetwork.SelectedServer = SelectedNetwork.Servers
                .FirstOrDefault(s => s.Server?.DisplayName == selectedServer.DisplayName);
#endif
        }

        private void SaveNetworks(NetworksModifiedMessage m)
        {
#if WINDOWS_UWP
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            List<Network> networks = Networks?.Select(n => n.Network).ToList();
            roamingSettings.Values["Networks"] = Networks != null ? JsonConvert.SerializeObject(networks) : null;
            roamingSettings.Values["SelectedNetwork"] = SelectedNetwork != null
                ? JsonConvert.SerializeObject(SelectedNetwork.Network)
                : null;
            roamingSettings.Values["SelectedServer"] = SelectedNetwork?.SelectedServer?.Server != null
                ? JsonConvert.SerializeObject(SelectedNetwork.SelectedServer.Server)
                : null;
#endif
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
            if (IsDeletingNetwork)
            {
                Networks.Remove(SelectedNetwork);
                SelectedNetwork = null;
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Networks));
                IsDeletingNetwork = false;
            }
            else if (IsEditingNetwork)
            {
                NetworkViewModel editedNetwork = SelectedNetwork;
                SelectedNetwork = null;
                if (IsAddingNewNetwork)
                {
                    editedNetwork = new NetworkViewModel();
                    Networks.Add(editedNetwork);
                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(Networks));
                }
                ApplyNewNetworkProperties(editedNetwork);
                SelectedNetwork = editedNetwork;
            }
            CancelNetworkOperation();
            Send(new NetworksModifiedMessage());
        }
        
        private void CancelNetworkOperation()
        {
            if (IsDeletingNetwork)
            {
                IsDeletingNetwork = false;
            }
            else
            {
                IsAddingNewNetwork = false;
                IsEditingNetwork = false;
                ClearNetworkForm();
            }
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
            IsDeletingNetwork = true;
        }
    }
}
