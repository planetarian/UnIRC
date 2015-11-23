using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking;
using UnIRC.IrcEvents;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Messages;
using UnIRC.Shared.Models;

namespace UnIRC.ViewModels
{
    public class ConnectionViewModel : ViewModelBaseExtended
    {
        private const int SleepMillis = 2;

        private static int _nextConnectionId = 1;

        public int ConnectionId
        {
            get { return _connectionId; }
            set { Set(ref _connectionId, value); }
        }
        private int _connectionId;

        public Network Network
        {
            get { return _network; }
            set { Set(ref _network, value); }
        }
        private Network _network;

        public ObservableCollection<Server> Servers
        {
            get { return _servers; }
            set { Set(ref _servers, value); }
        }
        private ObservableCollection<Server> _servers;



        public Server Server
        {
            get { return _server; }
            set { Set(ref _server, value); }
        }
        private Server _server;

        public string Address
        {
            get { return _address; }
            set { Set(ref _address, value); }
        }
        private string _address;

        public int Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }
        private int _port;

        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }
        private string _password;




        public UserInfo UserInfo
        {
            get { return _userInfo; }
            set { Set(ref _userInfo, value); }
        }
        private UserInfo _userInfo;


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
        
        public string DefaultNick
        {
            get { return _defaultNick; }
            set { Set(ref _defaultNick, value); }
        }
        private string _defaultNick;

        public string BackupNick
        {
            get { return _backupNick; }
            set { Set(ref _backupNick, value); }
        }
        private string _backupNick;


        public string DisplayName
        {
            get { return _displayName; }
            set { Set(ref _displayName, value); }
        }
        private string _displayName;

        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }
        private bool _isConnected;

        public int UnreadMessages
        {
            get { return _unreadMessages; }
            set { Set(ref _unreadMessages, value); }
        }
        private int _unreadMessages;

        public IConnectionEndpoint Endpoint
        {
            get { return _endpoint; }
            set { Set(ref _endpoint, value); }
        }
        private IConnectionEndpoint _endpoint;

        public int LastSuccessfulPort
        {
            get { return _lastSuccessfulPort; }
            set { Set(ref _lastSuccessfulPort, value); }
        }
        private int _lastSuccessfulPort;

        public ObservableCollection<IrcEvent> MessagesReceived
        {
            get { return _messagesReceived; }
            set { Set(ref _messagesReceived, value); }
        }
        private ObservableCollection<IrcEvent> _messagesReceived
            = new ObservableCollection<IrcEvent>();

        public List<string> MessagesSent
        {
            get { return _messagesSent; }
            set { Set(ref _messagesSent, value); }
        }
        private List<string> _messagesSent
            = new List<string>();

        public int CurrentMessageHistoryIndex
        {
            get { return _currentMessageHistoryIndex; }
            set { Set(ref _currentMessageHistoryIndex, value); }
        }
        private int _currentMessageHistoryIndex;


        public string CurrentTypedMessage
        {
            get { return _currentTypedMessage; }
            set { Set(ref _currentTypedMessage, value); }
        }
        private string _currentTypedMessage;


        public string InputMessage
        {
            get { return _inputMessage; }
            set { Set(ref _inputMessage, value); }
        }
        private string _inputMessage;


        public ICommand ReconnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand PrevHistoryMessageCommand { get; set; }
        public ICommand NextHistoryMessageCommand { get; set; }

        public ConnectionViewModel(Network network, Server server, UserInfo userInfo, IConnectionEndpoint endpoint)
        {
            if (IsInDesignModeStatic || IsInDesignMode) return;

            ConnectionId = _nextConnectionId++;

            this.OnChanged(x => x.Network, x => x.Nick)
                .Do(() => DisplayName = $"{Network?.Name} ({Nick})");
            this.OnChanged(x => x.InputMessage).Do(
                () =>
                {
                    if (CurrentMessageHistoryIndex < MessagesSent.Count
                        && InputMessage != MessagesSent[CurrentMessageHistoryIndex])
                    {
                        CurrentTypedMessage = InputMessage;
                        CurrentMessageHistoryIndex = MessagesSent.Count;
                    }
                    else if (CurrentMessageHistoryIndex == MessagesSent.Count
                             && InputMessage != CurrentTypedMessage)
                    {
                        CurrentTypedMessage = InputMessage;
                    }
                });


            Register<NetworksModifiedMessage>(m =>
            {
                Server selected = Server;
                // ReSharper disable once ExplicitCallerInfoArgument
                Servers = Network.Servers.ToObservable();
                RaisePropertyChanged(nameof(Servers));
                if (Servers.Contains(selected))
                    Server = selected;
            });

            ReconnectCommand = GetCommand(async () => await Connect(),
                () => !IsConnected, () => IsConnected);
            DisconnectCommand = GetCommand(async () => await Disconnect(),
                () => IsConnected, () => IsConnected);
            SendMessageCommand = GetCommand(async () => await SendMessageAsync(),
                () => !InputMessage.IsNullOrEmpty(),
                () => InputMessage);
            PrevHistoryMessageCommand = GetCommand(PrevHistoryMessage,
                () => CurrentMessageHistoryIndex > 0,
                () => CurrentMessageHistoryIndex);
            NextHistoryMessageCommand = GetCommand(NextHistoryMessage,
                () => !InputMessage.IsNullOrEmpty() && CurrentMessageHistoryIndex <= MessagesSent.Count,
                () => CurrentMessageHistoryIndex);

            Network = network;
            Server = server;
            Endpoint = endpoint;
            UserInfo = userInfo;

            FullName = userInfo.FullName;
            EmailAddress = userInfo.EmailAddress;
            DefaultNick = userInfo.Nick;
            BackupNick = userInfo.BackupNick;

            // Set the starting nick, it will be changed if there's a clash anyway
            Nick = DefaultNick;

            Endpoint.CreateConnection(ConnectionId);

            Task connectTask = Connect();
            //*/
        }

        private void PrevHistoryMessage()
        {
            InputMessage = MessagesSent[--CurrentMessageHistoryIndex];
        }

        private void NextHistoryMessage()
        {
            CurrentMessageHistoryIndex++;
            if (CurrentMessageHistoryIndex < MessagesSent.Count)
                InputMessage = MessagesSent[CurrentMessageHistoryIndex];
            else if (CurrentMessageHistoryIndex == MessagesSent.Count)
                InputMessage = CurrentTypedMessage;
            else
            {
                MessagesSent.Add(CurrentTypedMessage);
                InputMessage = "";
            };
        }

        private async Task Connect()
        {
            DisplayEvent("[ Connecting... ]");

            // Copy the data so that it stays current even if we edit the server data in the meantime
            Address = Server.Address;
            Port = LastSuccessfulPort;
            Password = Server.Password;
            if (LastSuccessfulPort == 0)
            {
                IEnumerable<int> allPorts = Server.AllPorts.ToArray();
                Port = allPorts.Last() <= LastSuccessfulPort ? allPorts.First() : allPorts.FirstOrDefault(p => p > LastSuccessfulPort);
            }

            try {
                await Endpoint.ConnectAsync(ConnectionId, Address, Port);
                IsConnected = true;
                if (!Password.IsNullOrEmpty())
                    await SendMessageAsync($"PASS {Password}");
                await SendMessageAsync($"NICK {DefaultNick}");
                HostName localAddress = Endpoint.GetLocalAddress(ConnectionId);
                await SendMessageAsync($@"USER {EmailAddress} ""{localAddress}"" ""{Address}"" :{FullName}");
            }
            catch (Exception ex)
            {
                DisplayEvent($"[ Connect() Error: {ex.Message} ]");
            }

            await WaitForData();
        }

        private async Task Disconnect(string message = null)
        {
            const string defaultMessage = "User disconnected.";
            await SendMessageAsync($"QUIT :{message ?? defaultMessage}");
            await Task.Delay(1000);
            await Endpoint.DisconnectAsync(ConnectionId);
            IsConnected = false;
            DisplayEvent("[ Disconnected ]");
        }

        private async Task WaitForData()
        {
            while (IsConnected)
            {
                await Task.Delay(SleepMillis);

                string incomingMessage;
                try
                {
                    IrcEvent ev = await Endpoint.WaitForEventAsync(ConnectionId);
                    HandleEvent(ev);
                    continue;
                }
                catch (EndOfStreamException)
                {
                    IsConnected = false;
                    incomingMessage = "[ Socket Disconnected ]";
                }
                catch (Exception ex) when (ex.HResult == -2147014843)
                {
                    // We closed the connection and cleanly disposed the reader
                    IsConnected = false;
                    incomingMessage = $"[ Connection closed (socket aborted) ]";
                }
                catch (ObjectDisposedException)
                {
                    // We closed the connection and cleanly disposed the reader
                    IsConnected = false;
                    incomingMessage = $"[ Connection closed (object disposed) ]";
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    incomingMessage = $"[ WaitForData() Error: {ex.Message} ]";
                }
                DisplayEvent(incomingMessage);
            }
        }

        private async Task SendMessageAsync()
        {
            string inputMessage = InputMessage;
            MessagesSent.Add(inputMessage);
            CurrentMessageHistoryIndex = MessagesSent.Count;
            InputMessage = "";
            await SendMessageAsync(inputMessage);
        }

        private async Task SendMessageAsync(string message)
        {
            DisplayEvent($"< {message}");
            if (!IsConnected)
            {
                DisplayEvent($"[ Not connected to server. ]");
                return;
            }

            try
            {
                await Endpoint.SendStringAsync(ConnectionId, message);
            }
            catch (Exception ex)
            {
                IsConnected = false;
                DisplayEvent($"[ SendMessageAsync(message) Error: {ex.Message} ]");
            }
        }

        private void DisplayEvent(IrcEvent ev)
        {
            MessagesReceived.Add(ev);
        }

        private void DisplayEvent(string ev)
        {
            MessagesReceived.Add(IrcEvent.GetEvent(ev));
        }

        private void HandleEvent(IrcEvent ev)
        {
            DisplayEvent(ev);
            new TypeSwitch()
                .Case<IrcEvent>(e => { })
                .Case<IrcChannelModeEvent>(HandleEvent)
                .Case<IrcInviteEvent>(HandleEvent)
                .Case<IrcJoinEvent>(HandleEvent)
                .Case<IrcKickEvent>(HandleEvent)
                .Case<IrcNickEvent>(HandleEvent)
                .Case<IrcNoticeEvent>(HandleEvent)
                .Case<IrcPartEvent>(HandleEvent)
                .Case<IrcPrivmsgEvent>(HandleEvent)
                .Case<IrcQuitEvent>(HandleEvent)
                .Case<IrcUserModeEvent>(HandleEvent)
                .Case<IrcWhoEvent>(HandleEvent)
                .Case<IrcWhoItemEvent>(HandleEvent)
                .Switch(ev);
        }

        private void HandleEvent(IrcChannelModeEvent ev)
        {
        }

        private void HandleEvent(IrcInviteEvent ev)
        {
        }

        private void HandleEvent(IrcJoinEvent ev)
        {
        }

        private void HandleEvent(IrcKickEvent ev)
        {
        }

        private void HandleEvent(IrcNickEvent ev)
        {
            if (ev.OldNick == Nick)
                Nick = ev.NewNick;
        }

        private void HandleEvent(IrcNoticeEvent ev)
        {
        }

        private void HandleEvent(IrcPartEvent ev)
        {
        }

        private void HandleEvent(IrcPrivmsgEvent ev)
        {
        }

        private void HandleEvent(IrcQuitEvent ev)
        {
        }

        private void HandleEvent(IrcUserModeEvent ev)
        {
        }

        private void HandleEvent(IrcWhoEvent ev)
        {
        }

        private void HandleEvent(IrcWhoItemEvent ev)
        {
        }
    }
}
