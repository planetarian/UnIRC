using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Messages;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class NetworkViewModel : ViewModelBaseExtended
    {
        public Network Network
        {
            get { return _network; }
            set { Set(ref _network, value); }
        }
        private Network _network;

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
        private string _name;

        // Servers:

        public ObservableCollection<ServerViewModel> Servers
        {
            get { return _servers; }
            set { Set(ref _servers, value); }
        }
        private ObservableCollection<ServerViewModel> _servers
            = new ObservableCollection<ServerViewModel>();

        public ServerViewModel SelectedServer
        {
            get { return _selectedServer; }
            set { Set(ref _selectedServer, value); }
        }
        private ServerViewModel _selectedServer;

        // Server editing properties:

        public string NewServerName
        {
            get { return _newServerName; }
            set { Set(ref _newServerName, value); }
        }
        private string _newServerName;

        public string NewServerAddress
        {
            get { return _newServerAddress; }
            set { Set(ref _newServerAddress, value); }
        }
        private string _newServerAddress;

        public string NewServerPassword
        {
            get { return _newServerPassword; }
            set { Set(ref _newServerPassword, value); }
        }
        private string _newServerPassword;

        public ObservableCollection<PortRange> NewServerPorts
        {
            get { return _newServerPorts; }
            set { Set(ref _newServerPorts, value); }
        }
        private ObservableCollection<PortRange> _newServerPorts
            = new ObservableCollection<PortRange>();

        public string NewServerPortRange
        {
            get { return _newServerPortRange; }
            set { Set(ref _newServerPortRange, value); }
        }
        private string _newServerPortRange;

        public PortRange SelectedPortRange
        {
            get { return _selectedPortRange; }
            set { Set(ref _selectedPortRange, value); }
        }
        private PortRange _selectedPortRange;

        public bool NewServerUseSsl
        {
            get { return _newServerUseSsl; }
            set { Set(ref _newServerUseSsl, value); }
        }
        private bool _newServerUseSsl;

        public bool UseNetworkNick
        {
            get { return _useNetworkNick; }
            set { Set(ref _useNetworkNick, value); }
        }
        private bool _useNetworkNick;

        public string Nick
        {
            get { return _nick; }
            set { Set(ref _nick, value); }
        }
        private string _nick;

        public string BackupNick
        {
            get { return _backupNick; }
            set { Set(ref _backupNick, value); }
        }
        private string _backupNick;


        // State

        public bool IsAddingNewServer
        {
            get { return _isAddingNewServer; }
            set { Set(ref _isAddingNewServer, value); }
        }
        private bool _isAddingNewServer;

        public bool IsEditingServer
        {
            get { return _isEditingServer; }
            set { Set(ref _isEditingServer, value); }
        }
        private bool _isEditingServer;

        public bool IsDeletingServer
        {
            get { return _isDeletingServer; }
            set { Set(ref _isDeletingServer, value); }
        }
        private bool _isDeletingServer;


        // Server CRUD:

        public ICommand CreateNewServerCommand { get; set; }
        public ICommand EditServerCommand { get; set; }
        public ICommand SaveServerCommand { get; set; }
        public ICommand CancelServerOperationCommand { get; set; }
        public ICommand DeleteServerCommand { get; set; }

        public ICommand AddPortRangeCommand { get; set; }
        public ICommand DeleteSelectedPortRangeCommand { get; set; }


        public NetworkViewModel() : this(new Network())
        {
        }

        public NetworkViewModel(Network network)
        {
            Network = network;
            Name = network.Name;
            Servers = network.Servers?.Select(s => new ServerViewModel(s)).ToObservable()
                      ?? new ObservableCollection<ServerViewModel>();
            UseNetworkNick = network.UseNetworkNick;
            Nick = network.Nick;
            BackupNick = network.BackupNick;

            CreateNewServerCommand = GetCommand(CreateNewServer);
            EditServerCommand = GetCommand(EditServer, () => SelectedServer != null, () => SelectedServer);
            SaveServerCommand = GetCommand(SaveServer, () => IsDeletingServer || ValidateNewServerData(),
                () => NewServerName, () => NewServerAddress, () => NewServerPorts, () => NewServerUseSsl,
                () => IsDeletingServer);
            CancelServerOperationCommand = GetCommand(CancelServerOperation);
            DeleteServerCommand = GetCommand(DeleteSelectedServer, () => SelectedServer != null, () => SelectedServer);
            AddPortRangeCommand = GetCommand(AddNewServerPortRange,
                ValidateNewServerPortRange, () => NewServerPortRange);
            DeleteSelectedPortRangeCommand = GetCommand(DeleteSelectedPortRange, () => SelectedPortRange != null,
                () => SelectedPortRange);
            
            this.OnChanged(x => x.Name).Do(() => Network.Name = Name);
            this.OnChanged(x => x.Servers).Do(() => Network.Servers = Servers?.Select(s => s.Server).ToList());
            this.OnChanged(x => x.SelectedServer, x => x.SelectedPortRange).Do(() => Send(new NetworksModifiedMessage()));
            this.OnChanged(x => x.UseNetworkNick).Do(() => Network.UseNetworkNick = UseNetworkNick);
            this.OnChanged(x => x.Nick).Do(() => Network.Nick = Nick);
            this.OnChanged(x => x.BackupNick).Do(() => Network.BackupNick = BackupNick);
        }


        // Network CRUD:

        private void CreateNewServer()
        {
            ClearServerForm();
            IsAddingNewServer = true;
            IsEditingServer = true;
        }

        private void EditServer()
        {
            GetServerProperties(SelectedServer);
            IsEditingServer = true;
        }

        private void SaveServer()
        {
            if (IsDeletingServer)
            {
                Servers.Remove(SelectedServer);
                SelectedServer = null;
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(Servers));
                IsDeletingServer = false;
            }
            else if (IsEditingServer)
            {
                ServerViewModel editedServer = SelectedServer;
                SelectedServer = null;
                if (IsAddingNewServer)
                {
                    editedServer = new ServerViewModel();
                    Servers.Add(editedServer);
                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(Servers));
                }
                ApplyNewServerProperties(editedServer);
                SelectedServer = editedServer;
            }
            CancelServerOperation();
            Send(new NetworksModifiedMessage());
        }

        private void CancelServerOperation()
        {
            if (IsDeletingServer)
            {
                IsDeletingServer = false;
            }
            else
            {
                IsAddingNewServer = false;
                IsEditingServer = false;
                ClearServerForm();
            }
        }

        private void GetServerProperties(ServerViewModel sourceServer)
        {
            NewServerName = sourceServer?.Name;
            NewServerAddress = sourceServer?.Address;
            NewServerPassword = sourceServer?.Password;
            NewServerPorts = sourceServer?.Ports.ToObservable();
            NewServerUseSsl = sourceServer?.UseSsl ?? false;
        }

        private void ApplyNewServerProperties(ServerViewModel targetServer)
        {
            targetServer.Name = NewServerName;
            targetServer.Address = NewServerAddress;
            targetServer.Password = NewServerPassword;
            targetServer.Ports = NewServerPorts;
            targetServer.UseSsl = NewServerUseSsl;
        }

        private void ClearServerForm()
        {
            NewServerName = "";
            NewServerAddress = "";
            NewServerPassword = "";
            NewServerPortRange = "";
            NewServerPorts = new ObservableCollection<PortRange>();
            NewServerUseSsl = false;
        }

        private void DeleteSelectedServer()
        {
            IsDeletingServer = true;
        }

        private bool ValidateNewServerData()
        {
            string serverDisplayName = Server.GetServerDisplayName(NewServerAddress, NewServerPorts, NewServerUseSsl);
            string newName = NewServerName.IsNullOrWhitespace() ? serverDisplayName : NewServerName;
            return !NewServerAddress.IsNullOrEmpty()
                   && NewServerPorts != null
                   && NewServerPorts.Count > 0
                   && Servers != null
                   && !Servers
                       .Where(s => s.Server.DisplayName != SelectedServer.Server.DisplayName)
                       .Select(s => s.DisplayName)
                       .Contains(newName);
        }

        private bool ValidateNewServerPortRange()
        {
            const string validPortRangeRegex = @"^((\d{1,5})|(\d{1,5}-\d{1,5}))(,((\d{1,5})|(\d{1,5}-\d{1,5})))*$";
            return NewServerPortRange != null
                   && Regex.IsMatch(NewServerPortRange, validPortRangeRegex, RegexOptions.Compiled);
        }

        private void AddNewServerPortRange()
        {
            string[] splitPortRanges = NewServerPortRange.Split(',');
            const string validInputRangeRegex = @"^(?:(?<single>\d{1,5})|(?:(?<start>\d{1,5})-(?<end>\d{1,5})))$";
            foreach (string inputRange in splitPortRanges)
            {
                Match match = Regex.Match(inputRange, validInputRangeRegex, RegexOptions.Compiled);
                if (!match.Success) continue;

                Group matchedSingle = match.Groups["single"];
                if (matchedSingle.Success)
                {
                    int single = Int32.Parse(matchedSingle.Value);
                    NewServerPorts.Add(new PortRange(single));
                    continue;
                }

                Group matchedStart = match.Groups["start"];
                Group matchedEnd = match.Groups["end"];
                int start = Int32.Parse(matchedStart.Value);
                int end = Int32.Parse(matchedEnd.Value);
                if (end < start)
                    continue;
                NewServerPorts.Add(new PortRange(start, end));
            }
            NewServerPortRange = "";
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(NewServerPorts));
        }

        private void DeleteSelectedPortRange()
        {
            NewServerPorts.Remove(SelectedPortRange);
            SelectedPortRange = null;
        }

    }
}
