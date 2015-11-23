using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnIRC.Models;
using UnIRC.Shared.Helpers;

namespace UnIRC.ViewModels
{
    public class ServerViewModel : ViewModelBaseExtended
    {
        public Server Server
        {
            get { return _server; }
            set { Set(ref _server, value); }
        }
        private Server _server;
        
        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }
        private string _name;

        public string Address
        {
            get { return _address; }
            set { Set(ref _address, value); }
        }
        private string _address;

        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }
        private string _password;

        public ObservableCollection<PortRange> Ports
        {
            get { return _ports; }
            set { Set(ref _ports, value); }
        }
        private ObservableCollection<PortRange> _ports;

        public bool UseSsl
        {
            get { return _useSsl; }
            set { Set(ref _useSsl, value); }
        }
        private bool _useSsl;

        public string DisplayName => Name.IsNullOrWhitespace() ? Server.ToString() : Name;


        public ServerViewModel() : this(new Server())
        {
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        public ServerViewModel(Server server)
        {
            Server = server;
            Name = server.Name;
            Address = server.Address;
            UseSsl = server.UseSsl;
            Password = server.Password;
            Ports = server.Ports?.ToObservable() ?? new ObservableCollection<PortRange>();

            this.OnChanged(x => x.Name).Do(() => Server.Name = Name);
            this.OnChanged(x => x.Address).Do(() => Server.Address = Address);
            this.OnChanged(x => x.Password).Do(() => Server.Password = Password);
            this.OnChanged(x => x.Ports).Do(() => Server.Ports = Ports?.ToList());
            this.OnChanged(x => x.UseSsl).Do(() => Server.UseSsl = UseSsl);
            this.OnChanged(x => x.Name, x => x.Address, x => x.UseSsl, x => x.Ports)
                .Do(() => RaisePropertyChanged(nameof(DisplayName)));
        }

    }
}
