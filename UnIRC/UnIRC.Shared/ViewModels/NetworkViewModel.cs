using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Models;
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

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        // Servers:

        private ObservableCollection<ServerViewModel> _servers
            = new ObservableCollection<ServerViewModel>();
        public ObservableCollection<ServerViewModel> Servers
        {
            get { return _servers; }
            set { Set(nameof(Servers), ref _servers, value); }
        }

        private ServerViewModel _selectedServer;
        public ServerViewModel SelectedServer
        {
            get { return _selectedServer; }
            set { Set(nameof(SelectedServer), ref _selectedServer, value); }
        }

        // Server editing properties:

        private string _newServerName;
        public string NewServerName
        {
            get { return _newServerName; }
            set { Set(nameof(NewServerName), ref _newServerName, value); }
        }

        private string _newServerAddress;
        public string NewServerAddress
        {
            get { return _newServerAddress; }
            set { Set(nameof(NewServerAddress), ref _newServerAddress, value); }
        }

        private string _newServerPassword;
        public string NewServerPassword
        {
            get { return _newServerPassword; }
            set { Set(nameof(NewServerPassword), ref _newServerPassword, value); }
        }

        private ObservableCollection<PortRange> _newServerPorts
            = new ObservableCollection<PortRange>();
        public ObservableCollection<PortRange> NewServerPorts
        {
            get { return _newServerPorts; }
            set { Set(nameof(NewServerPorts), ref _newServerPorts, value); }
        }

        private string _newServerPortRange;
        public string NewServerPortRange
        {
            get { return _newServerPortRange; }
            set { Set(nameof(NewServerPortRange), ref _newServerPortRange, value); }
        }

        private PortRange _selectedPortRange;
        public PortRange SelectedPortRange
        {
            get { return _selectedPortRange; }
            set { Set(nameof(SelectedPortRange), ref _selectedPortRange, value); }
        }

        // State

        private bool _isAddingNewServer;
        public bool IsAddingNewServer
        {
            get { return _isAddingNewServer; }
            set { Set(nameof(IsAddingNewServer), ref _isAddingNewServer, value); }
        }

        private bool _isEditingServer;
        public bool IsEditingServer
        {
            get { return _isEditingServer; }
            set { Set(nameof(IsEditingServer), ref _isEditingServer, value); }
        }
        
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

            this.OnChanged(x => x.Name).Do(() => Network.Name = Name);
            
            CreateNewServerCommand = GetCommand(CreateNewServer);
            EditServerCommand = GetCommand(EditServer, () => SelectedServer != null, () => SelectedServer);
            SaveServerCommand = GetCommand(SaveServer, ()=> !NewServerAddress.IsNullOrEmpty() && NewServerPorts != null && NewServerPorts.Count > 0, () => NewServerAddress, () => NewServerPorts);
            CancelServerOperationCommand = GetCommand(CancelServerOperation);
            DeleteServerCommand = GetCommand(DeleteSelectedServer, () => SelectedServer != null, () => SelectedServer);
            AddPortRangeCommand = GetCommand(AddNewServerPortRange,
                ValidateNewServerPortRange, () => NewServerPortRange);
            DeleteSelectedPortRangeCommand = GetCommand(DeleteSelectedPortRange, () => SelectedPortRange != null,
                () => SelectedPortRange);
        }

        private bool ValidateSaveServer()
        {
            return !NewServerAddress.IsNullOrEmpty() && NewServerPorts != null && NewServerPorts.Count > 0;
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
            if (IsEditingServer)
            {
                ServerViewModel editedServer = SelectedServer;
                SelectedServer = null;
                if (IsAddingNewServer)
                {
                    editedServer = new ServerViewModel();
                    Servers.Add(editedServer);
                }
                ApplyNewServerProperties(editedServer);
                SelectedServer = editedServer;
            }
            CancelServerOperation();
        }

        private void CancelServerOperation()
        {
            IsAddingNewServer = false;
            IsEditingServer = false;
            ClearServerForm();
        }

        private void GetServerProperties(ServerViewModel sourceServer)
        {
            NewServerName = sourceServer?.Name;
            NewServerAddress = sourceServer?.Address;
            NewServerPassword = sourceServer?.Password;
            NewServerPorts = sourceServer?.Ports;
        }

        private void ApplyNewServerProperties(ServerViewModel targetServer)
        {
            targetServer.Name = NewServerName;
            targetServer.Address = NewServerAddress;
            targetServer.Password = NewServerPassword;
            targetServer.Ports = NewServerPorts;
        }

        private void ClearServerForm()
        {
            NewServerName = "";
            NewServerAddress = "";
            NewServerPassword = "";
            NewServerPortRange = "";
            NewServerPorts = new ObservableCollection<PortRange>();
        }

        private void DeleteSelectedServer()
        {
            Servers.Remove(SelectedServer);
            SelectedServer = null;
        }

        private bool ValidateNewServerPortRange()
        {
            const string validPortRangeRegex = @"^((\d{1,5})|(\d{1,5}-\d{1,5}))(,((\d{1,5})|(\d{1,5}-\d{1,5})))*$";
            return Regex.IsMatch(NewServerPortRange, validPortRangeRegex, RegexOptions.Compiled);
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
            RaisePropertyChanged(nameof(NewServerPorts));
        }

        private void DeleteSelectedPortRange()
        {
            NewServerPorts.Remove(SelectedPortRange);
            SelectedPortRange = null;
        }

    }
}
