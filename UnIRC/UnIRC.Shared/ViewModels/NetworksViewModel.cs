
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

        public string Nick
        {
            get { return _nick; }
            set { Set(ref _nick, value); }
        }
        private string _nick;

        public string FullName
        {
            get { return _fullName; }
            set { Set(ref _fullName, value); }
        }
        private string _fullName;

        public string EmailAddress
        {
            get { return _emailAddress; }
            set { Set(ref _emailAddress, value); }
        }
        private string _emailAddress;


        public string BackupNick
        {
            get { return _backupNick; }
            set { Set(ref _backupNick, value); }
        }
        private string _backupNick;

        public string GlobalNick
        {
            get { return _globalNick; }
            set { Set(ref _globalNick, value); }
        }
        private string _globalNick;

        public string GlobalBackupNick
        {
            get { return _globalBackupNick; }
            set { Set(ref _globalBackupNick, value); }
        }
        private string _globalBackupNick;

        public string GlobalFullName
        {
            get { return _globalFullName; }
            set { Set(ref _globalFullName, value); }
        }
        private string _globalFullName;

        public string GlobalEmailAddress
        {
            get { return _globalEmailAddress; }
            set { Set(ref _globalEmailAddress, value); }
        }
        private string _globalEmailAddress;



        public bool EnableInvisibleMode
        {
            get { return _enableInvisibleMode; }
            set { Set(ref _enableInvisibleMode, value); }
        }
        private bool _enableInvisibleMode = true;


        public bool UseNetworkNick
        {
            get { return _useNetworkNick; }
            set { Set(ref _useNetworkNick, value); }
        }
        private bool _useNetworkNick;
        
        public bool IsNetworkNickAvailable
        {
            get { return _isNetworkNickAvailable; }
            set { Set(ref _isNetworkNickAvailable, value); }
        }
        private bool _isNetworkNickAvailable;

        public bool NewNetworkUseNetworkNick
        {
            get { return _newNetworkUseNetworkNick; }
            set { Set(ref _newNetworkUseNetworkNick, value); }
        }
        private bool _newNetworkUseNetworkNick;

        public string NewNetworkFullName
        {
            get { return _newNetworkFullName; }
            set { Set(ref _newNetworkFullName, value); }
        }
        private string _newNetworkFullName;

        public string NewNetworkEmailAddress
        {
            get { return _newNetworkEmailAddress; }
            set { Set(ref _newNetworkEmailAddress, value); }
        }
        private string _newNetworkEmailAddress;
        
        public string NewNetworkNick
        {
            get { return _newNetworkNick; }
            set { Set(ref _newNetworkNick, value); }
        }
        private string _newNetworkNick;

        public string NewNetworkBackupNick
        {
            get { return _newNetworkBackupNick; }
            set { Set(ref _newNetworkBackupNick, value); }
        }
        private string _newNetworkBackupNick;

        public bool CanEditNewNetworkNick
        {
            get { return _canEditNewNetworkNick; }
            set { Set(ref _canEditNewNetworkNick, value); }
        }
        private bool _canEditNewNetworkNick;



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
#if WINDOWS_UWP
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
#endif

            CreateNewNetworkCommand = GetCommand(CreateNewNetwork);
            EditNetworkCommand = GetCommand(EditNetwork, () => SelectedNetwork != null, () => SelectedNetwork);
            SaveNetworkCommand = GetCommand(SaveNetwork, () => !NewNetworkName.IsNullOrEmpty(), () => NewNetworkName);
            CancelNetworkOperationCommand = GetCommand(CancelNetworkOperation);
            DeleteNetworkCommand = GetCommand(DeleteSelectedNetwork, () => SelectedNetwork != null, () => SelectedNetwork);

            Register<NetworksModifiedMessage>(SaveNetworks);

            this.OnChanged(x => x.IsDeletingNetwork, x => x.NewNetworkUseNetworkNick)
                .Do(() => CanEditNewNetworkNick = !IsDeletingNetwork && NewNetworkUseNetworkNick);
            // Switch nicks when we decide whether to use server nick
            this.OnChanged(x => x.UseNetworkNick)
                .Do(() =>
                {
                    if (IsNetworkNickAvailable && UseNetworkNick)
                    {
                        FullName = SelectedNetwork.FullName;
                        EmailAddress = SelectedNetwork.EmailAddress;
                        Nick = SelectedNetwork.Nick;
                        BackupNick = SelectedNetwork.BackupNick;
                    }
                    else
                    {
                        FullName = GlobalFullName;
                        EmailAddress = GlobalEmailAddress;
                        Nick = GlobalNick;
                        BackupNick = GlobalBackupNick;
                    }
                });
            this.OnChanged(x => x.SelectedNetwork)
                .Do(() =>
                {
                    IsNetworkNickAvailable = SelectedNetwork?.UseNetworkNick ?? false;
                    UseNetworkNick = IsNetworkNickAvailable;
                    if (SelectedNetwork == null) return;
                    SelectedNetwork.SelectedServer = SelectedNetwork.Servers?.FirstOrDefault();
                    SelectedNetwork.CancelServerOperationCommand.Execute();
                });

#if WINDOWS_UWP
            var networksJson = roamingSettings.Values["Networks"] as string;
            var selectedNetworkJson = roamingSettings.Values["SelectedNetwork"] as string;
            var selectedServerJson = roamingSettings.Values["SelectedServer"] as string;
            var globalFullName = roamingSettings.Values["GlobalFullName"] as string;
            var globalEmailAddress = roamingSettings.Values["GlobalEmailAddress"] as string;
            var globalNick = roamingSettings.Values["GlobalNick"] as string;
            var globalBackupNick = roamingSettings.Values["GlobalBackupNick"] as string;
            var enableInvisibleMode = roamingSettings.Values["EnableInvisibleMode"] as string;

            FullName = GlobalFullName = globalFullName;
            EmailAddress = GlobalEmailAddress = globalEmailAddress;
            Nick = GlobalNick = globalNick;
            BackupNick = GlobalBackupNick = globalBackupNick;
            EnableInvisibleMode = enableInvisibleMode == "true";

            if (networksJson != null)
            {
                var networks = JsonConvert.DeserializeObject<List<Network>>(networksJson);
                Networks = networks.Select(n => new NetworkViewModel(n)).ToObservable();

                if (selectedNetworkJson != null)
                {
                    var selectedNetwork = JsonConvert.DeserializeObject<Network>(selectedNetworkJson);
                    SelectedNetwork = Networks.FirstOrDefault(n => n.Network?.Name == selectedNetwork.Name);

                    if (selectedServerJson != null)
                    {
                        var selectedServer = JsonConvert.DeserializeObject<Server>(selectedServerJson);
                        SelectedNetwork.SelectedServer = SelectedNetwork.Servers
                            .FirstOrDefault(s => s.Server?.DisplayName == selectedServer.DisplayName);
                    } // SelectedServer
                } // SelectedNetwork
            } // Networks
#endif
            //
            // Save state on modifications
            //

#if WINDOWS_UWP
            this.OnChanged(x => x.EnableInvisibleMode)
                .Do(() => roamingSettings.Values["EnableInvisibleMode"] = EnableInvisibleMode ? "true" : "false");
            this.OnChanged(x => x.SelectedNetwork)
                .Do(() =>
                {
                    roamingSettings.Values["SelectedNetwork"] = SelectedNetwork != null
                        ? JsonConvert.SerializeObject(SelectedNetwork.Network)
                        : null;
                });
#endif
            // Save nicks when they change
            this.OnChanged(x => x.FullName)
                .Do(() =>
                {
                    if (UseNetworkNick || FullName == GlobalFullName) return;
                    GlobalFullName = FullName;
#if WINDOWS_UWP
                    roamingSettings.Values["GlobalFullName"] = GlobalFullName;
#endif
                });
            this.OnChanged(x => x.EmailAddress)
                .Do(() =>
                {
                    if (UseNetworkNick || EmailAddress == GlobalEmailAddress) return;
                    GlobalEmailAddress = EmailAddress;
#if WINDOWS_UWP
                    roamingSettings.Values["GlobalEmailAddress"] = GlobalEmailAddress;
#endif
                });
            this.OnChanged(x => x.Nick)
                .Do(() =>
                {
                    if (UseNetworkNick || Nick == GlobalNick) return;
                    GlobalNick = Nick;
#if WINDOWS_UWP
                    roamingSettings.Values["GlobalNick"] = GlobalNick;
#endif
                });
            this.OnChanged(x => x.BackupNick)
                .Do(() =>
                {
                    if (UseNetworkNick || BackupNick == GlobalBackupNick) return;
                    GlobalBackupNick = BackupNick;
#if WINDOWS_UWP
                    roamingSettings.Values["GlobalBackupNick"] = GlobalBackupNick;
#endif
                });
        }

        private void SaveNetworks(NetworksModifiedMessage m)
        {
#if WINDOWS_UWP
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            List<Network> networks = Networks?.Select(n => n.Network).ToList();
            roamingSettings.Values["Networks"] = Networks != null ? JsonConvert.SerializeObject(networks) : null;
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
            NewNetworkUseNetworkNick = sourceNetwork?.UseNetworkNick ?? false;
            NewNetworkFullName = sourceNetwork?.FullName;
            NewNetworkEmailAddress = sourceNetwork?.EmailAddress;
            NewNetworkNick = sourceNetwork?.Nick;
            NewNetworkBackupNick = sourceNetwork?.BackupNick;
        }

        private void ApplyNewNetworkProperties(NetworkViewModel targetNetwork)
        {
            targetNetwork.Name = NewNetworkName;
            targetNetwork.UseNetworkNick = NewNetworkUseNetworkNick;
            targetNetwork.FullName = NewNetworkFullName;
            targetNetwork.EmailAddress = NewNetworkEmailAddress;
            targetNetwork.Nick = NewNetworkNick;
            targetNetwork.BackupNick = NewNetworkBackupNick;
        }

        private void ClearNetworkForm()
        {
            NewNetworkName = "";
            NewNetworkUseNetworkNick = false;
            NewNetworkFullName = "";
            NewNetworkEmailAddress = "";
            NewNetworkNick = "";
            NewNetworkBackupNick = "";
        }

        private void DeleteSelectedNetwork()
        {
            IsDeletingNetwork = true;
        }
    }
}
