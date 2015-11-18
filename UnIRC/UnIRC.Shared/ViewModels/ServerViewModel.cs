using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Models;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ServerViewModel : ViewModelBaseExtended
    {
        private Server _server;
        public Server Server
        {
            get { return _server; }
            private set { Set(nameof(Server), ref _server, value); }
        }
        
        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(nameof(Name), ref _name, value); }
        }

        public string DisplayName => Name.IsNullOrWhitespace() ? Address : Name;

        private string _address;
        public string Address
        {
            get { return _address; }
            set { Set(nameof(Address), ref _address, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { Set(nameof(Password), ref _password, value); }
        }

        private ObservableCollection<PortRange> _ports;
        public ObservableCollection<PortRange> Ports
        {
            get { return _ports; }
            set { Set(nameof(Ports), ref _ports, value); }
        }

        public ServerViewModel() : this(new Server())
        {
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        public ServerViewModel(Server server)
        {
            Server = server;
            Name = server.Name;
            Address = server.Address;
            Password = server.Password;
            Ports = server.Ports?.ToObservable();

            this.OnChanged(x => x.Name).Do(() => Server.Name = Name).Do(() => RaisePropertyChanged(nameof(DisplayName)));
            this.OnChanged(x => x.Address).Do(() => Server.Address = Address);
            this.OnChanged(x => x.Password).Do(() => Server.Password = Password);
            this.OnChanged(x => x.Ports).Do(() => Server.Ports = Ports.ToList());
        }
    }
}
