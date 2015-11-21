using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.Models;
using UnIRC.ViewModels;

namespace UnIRC.Shared.ViewModels
{
    public class ConnectionViewModel : ViewModelBaseExtended
    {
        private const int SleepMillis = 100;

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

        public Server Server
        {
            get { return _server; }
            set { Set(ref _server, value); }
        }
        private Server _server;

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

        public ObservableCollection<string> MessagesReceived
        {
            get { return _messagesReceived; }
            set { Set(ref _messagesReceived, value); }
        }
        private ObservableCollection<string> _messagesReceived
            = new ObservableCollection<string>();

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
            ConnectionId = _nextConnectionId++;

            this.OnChanged(x => x.Network).Do(() => DisplayName = Network?.Name);
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

            Endpoint.CreateConnection(ConnectionId);

            Task connectTask = Connect();
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
            MessagesReceived.Add("[ Connecting... ]");
            int port = LastSuccessfulPort;
            if (LastSuccessfulPort == 0)
            {
                IEnumerable<int> allPorts = Server.AllPorts.ToArray();
                port = allPorts.Last() <= LastSuccessfulPort ? allPorts.First() : allPorts.FirstOrDefault(p => p > LastSuccessfulPort);
            }
            try {
                await Endpoint.ConnectAsync(ConnectionId, Server.Address, port);
                IsConnected = true;
                if (!Server.Password.IsNullOrEmpty())
                    await SendMessageAsync($"PASS {Server.Password}");
                await SendMessageAsync($"NICK {DefaultNick}");
                HostName localAddress = Endpoint.GetLocalAddress(ConnectionId);
                await SendMessageAsync($@"USER {EmailAddress} ""{localAddress}"" ""{Server.Address}"" :{FullName}");
            }
            catch (Exception ex)
            {
                MessagesReceived.Add($"[ Connect() Error: {ex.Message} ]");
            }

            await WaitForData();
        }

        private async Task Disconnect()
        {
            await Endpoint.DisconnectAsync(ConnectionId);
            IsConnected = false;
            MessagesReceived.Add("[ Disconnected ]");
        }

        private async Task WaitForData()
        {
            while (IsConnected)
            {
                await Task.Delay(SleepMillis);

                string incomingMessage;
                try
                {
                    incomingMessage = await Endpoint.ReadStringAsync(ConnectionId);
                }
                catch (EndOfStreamException ex)
                {
                    IsConnected = false;
                    incomingMessage = "[ Socket Disconnected ]";
                }
                catch (ObjectDisposedException ex)
                {
                    // We closed the connection and cleanly disposed the reader
                    continue;
                }
                catch (Exception ex)
                {
                    incomingMessage = $"[ WaitForData() Error: {ex.Message} ]";
                }
                MessagesReceived.Add(incomingMessage);
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
            MessagesReceived.Add($"< {message}");
            await Endpoint.SendStringAsync(ConnectionId, message);
        }
    }
}
