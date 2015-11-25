using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking;
using UnIRC.IrcEvents;
using UnIRC.Models;
using UnIRC.Shared.Helpers;
using UnIRC.Shared.IrcEvents;
using UnIRC.Shared.Messages;
using UnIRC.Shared.Models;

namespace UnIRC.ViewModels
{
    public class ConnectionViewModel : ViewModelBaseExtended
    {
        #region ViewModel properties

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


        public ObservableCollection<IrcEvent> Messages
        {
            get { return _messages; }
            set { Set(ref _messages, value); }
        }
        private ObservableCollection<IrcEvent> _messages
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


        public ObservableCollection<ChannelViewModel> Channels
        {
            get { return _channels; }
            set { Set(ref _channels, value); }
        }
        private ObservableCollection<ChannelViewModel> _channels
            = new ObservableCollection<ChannelViewModel>();

        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { Set(ref _selectedChannel, value); }
        }
        private ChannelViewModel _selectedChannel;


        public bool IsReconnecting
        {
            get { return _isReconnecting; }
            set { Set(ref _isReconnecting, value); }
        }
        private bool _isReconnecting;

        public bool IsConnecting
        {
            get { return _isConnecting; }
            set { Set(ref _isConnecting, value); }
        }
        private bool _isConnecting;

        public bool IsConnectionOpen
        {
            get { return _isConnectionOpen; }
            set { Set(ref _isConnectionOpen, value); }
        }
        private bool _isConnectionOpen;

        public bool ShowDisconnect
        {
            get { return _showDisconnect; }
            set { Set(ref _showDisconnect, value); }
        }
        private bool _showDisconnect;

        public bool ShowReconnect
        {
            get { return _showReconnect; }
            set { Set(ref _showReconnect, value); }
        }
        private bool _showReconnect;

        public ObservableCollection<string> Motd
        {
            get { return _motd; }
            set { Set(ref _motd, value); }
        }
        private ObservableCollection<string> _motd
            = new ObservableCollection<string>();

        public bool IsActive
        {
            get { return _isActive; }
            set { Set(ref _isActive, value); }
        }
        private bool _isActive;



        public ICommand ReconnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand SendMessageCommand { get; set; }
        public ICommand PrevHistoryMessageCommand { get; set; }
        public ICommand NextHistoryMessageCommand { get; set; }

        #endregion ViewModel properties


        private readonly Dictionary<string, List<IrcUserNamesEntry>> _namesQueue
            = new Dictionary<string, List<IrcUserNamesEntry>>();
        private readonly Dictionary<string, List<IrcUserWhoEntry>> _whoQueue
            = new Dictionary<string, List<IrcUserWhoEntry>>();
        private readonly Queue<IrcEvent> _eventQueue
            = new Queue<IrcEvent>();
        

        private readonly SemaphoreSlim _eventProcessingLock = new SemaphoreSlim(1);
        private readonly object _messagesLock = new object();

        private const int _timerMillis = 5;
        private const int _sleepMillis = 10;
        private static int _nextConnectionId = 1;
        private const int _defaultReconnectSeconds = 3;
        private const int _reconnectInterval = 10;
        private const bool _reconnectMultiply = false;
        private int _reconnectSeconds = _defaultReconnectSeconds;
        private DateTime _nextReconnectDate;


        public ConnectionViewModel(Network network, Server server, UserInfo userInfo,
            IConnectionEndpoint endpoint)
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
            this.OnChanged(x => x.IsConnected, x => x.IsConnecting, x => x.IsReconnecting)
                .Do(() => ShowDisconnect = IsConnected || IsReconnecting);
            // ReSharper disable once ExplicitCallerInfoArgument
            this.OnCollectionChanged(x => x.Channels).Do(() => RaisePropertyChanged(nameof(Channels)));

            Register<NetworksModifiedMessage>(m =>
            {
                Server selected = Server;
                Servers = Network.Servers.ToObservable();
                if (Servers.Contains(selected))
                    Server = selected;
            });

            ReconnectCommand = GetCommand(async () => await Connect(),
                () => !IsConnected, () => IsConnected);
            DisconnectCommand = GetCommand(async () => await Quit(),
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
            Servers = network.Servers.ToObservable();
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
            
            Task processTask = ProcessEventsAsync();
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
            IsReconnecting = false;
            IsConnecting = true;
            if (IsConnected)
                await Disconnect();

            DisplayEvent("Connecting...");

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
                IsConnectionOpen = true;
                _reconnectSeconds = _defaultReconnectSeconds;
                if (!Password.IsNullOrEmpty())
                    await SendMessageAsync($"PASS {Password}");
                await SendMessageAsync($"NICK {DefaultNick}");
                HostName localAddress = Endpoint.GetLocalAddress(ConnectionId);
                await SendMessageAsync($@"USER {EmailAddress} ""{localAddress}"" ""{Address}"" :{FullName}");
            }
            catch (Exception ex)
            {
                DisplayEvent($"Connect() Error: {ex.Message}");
                IsReconnecting = true;
                ShowReconnectMessage();
            }

            await WaitForData();
        }

        private void ShowReconnectMessage()
        {
            if (DateTime.Now < _nextReconnectDate)
                DisplayEvent($"Reconnecting in {_nextReconnectDate - DateTime.Now:mm\\:ss}");
        }

        private async Task Quit(string message = null)
        {
            const string defaultMessage = "User disconnected.";
            await SendMessageAsync($"QUIT :{message ?? defaultMessage}");
            await Disconnect();
        }

        private async Task Disconnect()
        {
            IsConnected = false;
            IsConnectionOpen = false;
            await Endpoint.DisconnectAsync(ConnectionId);

            DisplayEvent("Disconnected.", true);

            _namesQueue.Clear();
            _whoQueue.Clear();
            lock(_eventProcessingLock)
                _eventQueue.Clear();

            foreach (ChannelViewModel channel in Channels)
            {
                channel.WasJoinedBeforeDisconnect = channel.IsJoined;
                channel.IsJoined = false;
            }
        }

        private void DisplayEvent(string message, bool global = false)
        {
            IrcEvent ev = IrcEvent.GetEvent(message);
            ev.EventType = IrcEventType.Internal;
            DisplayEvent(ev, global);
        }

        private void DisplayEvent(IrcEvent ev, bool global = false)
        {
            lock (_messagesLock)
            {
                Messages.Add(ev);
                if (!global) return;
                foreach(ChannelViewModel channel in Channels)
                    DisplayEvent(ev, channel);
            }
        }

        private void DisplayEvent(IrcEvent ev, ChannelViewModel channel)
        {
            lock (_messagesLock)
                channel.Messages.Add(ev);
        }

        private async Task ShowError(string message)
        {
            await HandleErrorEvent(new IrcErrorEvent(message) {EventType = IrcEventType.Internal});
        }

        public async Task SendMessageAsync()
        {
            string inputMessage = InputMessage;
            MessagesSent.Add(inputMessage);
            CurrentMessageHistoryIndex = MessagesSent.Count;
            InputMessage = "";
            await SendMessageAsync(inputMessage);
        }

        public async Task SendMessageAsync(string message)
        {
            //DisplayEvent($"-> {message}");
            if (!IsConnectionOpen)
            {
                await ShowError("Not connected to server.");
                return;
            }

            try
            {
                await Endpoint.SendStringAsync(ConnectionId, message);
            }
            catch (Exception ex)
            {
                await ShowError($"SendMessageAsync(message) Error: {ex.Message}");
            }

            IrcEvent ev = IrcEvent.GetEvent($":{Nick}!{UserInfo.EmailAddress} {message}");
            ev.EventType = IrcEventType.ToServer;
            lock (_eventProcessingLock)
                _eventQueue.Enqueue(ev);
        }

        private async Task WaitForData()
        {
            while (IsConnectionOpen)
            {
                await Task.Delay(_sleepMillis);

                string errorMessage;
                try
                {
                    IrcEvent ev = await Endpoint.WaitForEventAsync(ConnectionId);
                    lock (_eventProcessingLock)
                        _eventQueue.Enqueue(ev);
                    continue;
                }
                catch (EndOfStreamException) when (IsConnected)
                {
                    // Server closed the connection
                    errorMessage = "Socket Disconnected";
                }
                catch (Exception ex) when (IsConnected && ex.HResult == -2147014843)
                {
                    // We closed the connection and cleanly disposed the reader
                    errorMessage = "Connection closed (socket aborted)";
                }
                catch (ObjectDisposedException) when (IsConnected)
                {
                    // We closed the connection and cleanly disposed the reader
                    errorMessage = "Connection closed (object disposed)";
                }
                catch (COMException ex)
                {
                    errorMessage = $"Thread ended. {ex.Message}";
                }
                catch (Exception ex) when (IsConnected)
                {
                    // All other read errors
                    errorMessage = $"WaitForData() Error: {ex.Message}{Environment.NewLine}{ex}";
                }

                await Disconnect();
                await ShowError(errorMessage);
                IsReconnecting = true;
                ShowReconnectMessage();
            }
        }

        private async Task ProcessEventsAsync()
        {
            while (true)
            {
                IrcEvent newEvent = null;
                lock (_eventProcessingLock)
                {
                    if (_eventQueue.Count > 0)
                        newEvent = _eventQueue.Dequeue();
                }
                if (newEvent != null)
                {
                    try
                    {
                        await HandleIrcEventAsync(newEvent);
                    }
                    catch (InvalidDataException)
                    {
                        await ShowError($"IRC message has invalid format: {newEvent}");
                    }
                    catch (Exception ex)
                    {
                        await ShowError("Error occurred processing IRC event");
                        await ShowError($"Event: {newEvent}");
                        await ShowError($"Exception: {Environment.NewLine}{ex}");
                    }
                }

                if (!IsConnected && !IsConnecting && IsReconnecting && _nextReconnectDate < DateTime.Now)
                {
#pragma warning disable 162
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // ReSharper disable once UnreachableCode
                    int interval = _reconnectMultiply ? _reconnectSeconds*2 : _reconnectSeconds + _reconnectInterval;
#pragma warning restore 162
                    _nextReconnectDate = DateTime.Now + TimeSpan.FromSeconds(interval);
                    // ReSharper disable once UnusedVariable
                    Task connectTask = Connect();
                }

                await Task.Delay(_timerMillis);
            }
        }

        private async Task HandleIrcEventAsync(IrcEvent ev)
        {
            using (await _eventProcessingLock.LockAsync())
            {
                if (ev is IrcChannelModeEvent) await HandleChannelModeEvent((IrcChannelModeEvent) ev);
                else if (ev is IrcCreatedEvent) await HandleCreatedEvent((IrcCreatedEvent)ev);
                else if (ev is IrcErrorEvent) await HandleErrorEvent((IrcErrorEvent)ev);
                else if (ev is IrcInviteEvent) await HandleInviteEvent((IrcInviteEvent)ev);
                else if (ev is IrcJoinEvent) await HandleJoinEvent((IrcJoinEvent) ev);
                else if (ev is IrcKickEvent) await HandleKickEvent((IrcKickEvent) ev);
                else if (ev is IrcMotdBeginEvent) await HandleMotdBeginEvent((IrcMotdBeginEvent)ev);
                else if (ev is IrcMotdEvent) await HandleMotdEvent((IrcMotdEvent)ev);
                else if (ev is IrcMotdEndEvent) await HandleMotdEndEvent((IrcMotdEndEvent)ev);
                else if (ev is IrcNickEvent) await HandleNickEvent((IrcNickEvent)ev);
                else if (ev is IrcNamesItemEvent) await HandleNamesItemEvent((IrcNamesItemEvent) ev);
                else if (ev is IrcNamesEndEvent) await HandleNamesEndEvent((IrcNamesEndEvent) ev);
                else if (ev is IrcNoticeEvent) await HandleNoticeEvent((IrcNoticeEvent) ev);
                else if (ev is IrcPartEvent) await HandlePartEvent((IrcPartEvent) ev);
                else if (ev is IrcPingEvent) await HandlePingEvent((IrcPingEvent) ev);
                else if (ev is IrcPongEvent) await HandlePongEvent((IrcPongEvent) ev);
                else if (ev is IrcPrivmsgEvent) await HandlePrivmsgEvent((IrcPrivmsgEvent) ev);
                else if (ev is IrcQuitEvent) await HandleQuitEvent((IrcQuitEvent)ev);
                else if (ev is IrcServerInfoEvent) await HandleServerInfoEvent((IrcServerInfoEvent)ev);
                else if (ev is IrcUnknownCommandEvent) await HandleUnknownCommandEvent((IrcUnknownCommandEvent)ev);
                else if (ev is IrcUserEvent) await HandleUserEvent((IrcUserEvent)ev);
                else if (ev is IrcUserModeEvent) await HandleUserModeEvent((IrcUserModeEvent)ev);
                else if (ev is IrcWelcomeEvent) await HandleWelcomeEvent((IrcWelcomeEvent) ev);
                else if (ev is IrcWhoItemEvent) await HandleWhoItemEvent((IrcWhoItemEvent) ev);
                else if (ev is IrcWhoEndEvent) await HandleWhoEndEvent((IrcWhoEndEvent)ev);
                else if (ev is IrcYourHostEvent) await HandleYourHostEvent((IrcYourHostEvent)ev);
                else await HandleGenericEvent(ev);
                /*
                await new TypeSwitch()
                    .Case<IrcEvent>(e => { })
                    .Case<IrcChannelModeEvent>(HandleChannelModeEvent)
                    .Case<IrcInviteEvent>(HandleInviteEvent)
                    .Case<IrcJoinEvent>(HandleJoinEvent)
                    .Case<IrcKickEvent>(HandleKickEvent)
                    .Case<IrcNickEvent>(HandleNickEvent)
                    .Case<IrcNamesItemEvent>(HandleNamesItemEvent)
                    .Case<IrcNamesEndEvent>(HandleNamesEndEvent)
                    .Case<IrcNoticeEvent>(HandleNoticeEvent)
                    .Case<IrcPartEvent>(HandlePartEvent)
                    .Case<IrcPingEvent>(HandlePingEvent)
                    .Case<IrcPrivmsgEvent>(HandlePrivmsgEvent)
                    .Case<IrcQuitEvent>(HandleQuitEvent)
                    .Case<IrcUserModeEvent>(HandleUserModeEvent)
                    .Case<IrcWhoItemEvent>(HandleWhoItemEvent)
                    .Case<IrcWhoEndEvent>(HandleWhoEndEvent)
                    .SwitchAsync(ev);//*/
            }
        }
        
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task HandleChannelModeEvent(IrcChannelModeEvent ev)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;
            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                DisplayEvent("Received a MODE message for a channel we're not in!");
                return;
            }
            DisplayEvent(ev, channel);
        }

        private async Task HandleCreatedEvent(IrcCreatedEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleErrorEvent(IrcErrorEvent ev)
        {
            DisplayEvent(ev, true);
            if (ev.EventType == IrcEventType.FromServer
                && ev.Message.ToLower().StartsWith("closing link"))
                await Disconnect();
            IsReconnecting = true;
        }

        private async Task HandleInviteEvent(IrcInviteEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleJoinEvent(IrcJoinEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;
            ChannelViewModel channel = Channels
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (ev.User.Nick == Nick)
            {
                if (channel != null)
                {
                    if (channel.IsJoined)
                    {
                        DisplayEvent($"[ Received a JOIN message for {channelName} which we're already in! ]");
                        return;
                    }
                    channel.WasJoinedBeforeDisconnect = false;
                    channel.IsJoined = true;
                }
                else
                {
                    channel = new ChannelViewModel(this, channelName, ev.User);
                    Channels.Add(channel);
                }
            }
            else
            {
                if (!channel.IsJoined)
                {
                    DisplayEvent($"[ Received a JOIN message on {channelName} for {ev.User.Nick} but we're not joined! ]");
                    return;
                }
                if (channel.Users.Any(u => u.Nick == ev.User.Nick))
                {
                    DisplayEvent($"[ Received a JOIN message on {channelName} for {ev.User.Nick} who is already there! ]");
                    return;
                }
                channel.Users.Add(ev.User);
            }
            DisplayEvent(ev, channel);
        }

        private async Task HandleKickEvent(IrcKickEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;
            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                DisplayEvent($"[ Received a KICK message for {channelName} which we're not in! ]");
                return;
            }

            if (ev.Kicked.Contains(Nick)) // =(
            {
                channel.IsJoined = false;
            }
            else
            {
                foreach (var nick in ev.Kicked)
                {
                    if (channel.Users.All(u => u.Nick != nick))
                    {
                        DisplayEvent($"[ Received a KICK message for {nick} who isn't there! ]");
                        continue;
                    }
                    channel.Users.Remove(channel.Users.First(u => u.Nick == nick));
                }
            }
            DisplayEvent(ev, channel);
        }

        private async Task HandleMotdBeginEvent(IrcMotdBeginEvent ev)
        {
            Motd.Clear();
            DisplayEvent(ev);
        }

        private async Task HandleMotdEvent(IrcMotdEvent ev)
        {
            Motd.Add(ev.Message);
            DisplayEvent(ev);
        }

        private async Task HandleMotdEndEvent(IrcMotdEndEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleNamesItemEvent(IrcNamesItemEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            if (!_namesQueue.ContainsKey(ev.Channel))
                _namesQueue.Add(ev.Channel, new List<IrcUserNamesEntry>());
            _namesQueue[ev.Channel].AddRange(ev.Entries);

            DisplayEvent(ev);
        }

        private async Task HandleNamesEndEvent(IrcNamesEndEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;

            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
                return;

            List<IrcUserNamesEntry> entries;
            if (!_namesQueue.TryGetValue(channelName, out entries))
                return;

            IrcUser[] existingUsers = channel.Users.Where(u => entries.Any(e => e.Nick == u.Nick)).ToArray();
            IrcUserNamesEntry[] newEntries = entries.Where(e => channel.Users.All(u => u.Nick != e.Nick)).ToArray();

            _namesQueue[channelName].Clear();


            // Remove users that have left
            //foreach (IrcUser user in existingUsers)
                //newList.Remove(user);

            // Add users who have joined
            List<IrcUser> newList = existingUsers.ToList();
            newList.AddRange(newEntries.Select(entry => new IrcUser(entry.Nick, null, null, null, null)));

            channel.Users = newList.ToObservable();

            DisplayEvent(ev);
        }

        private async Task HandleNickEvent(IrcNickEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            if (ev.OldNick == Nick)
            {
                Nick = ev.NewNick;
                foreach (ChannelViewModel channel in Channels.Where(c => c.IsJoined))
                {
                    IrcUser user = channel.Users.First(u => u.Nick == ev.OldNick);
                    channel.Users.Remove(user);
                    channel.Users.Add(new IrcUser(ev.NewNick, user.UserName, user.Host, user.RealName, user.Server));
                    DisplayEvent(ev, channel);
                }
                DisplayEvent(ev);
            }
            else
            {
                bool found = false;
                foreach (ChannelViewModel channel in Channels.Where(c => c.IsJoined))
                {
                    IrcUser user = channel.Users.FirstOrDefault(u => u.Nick == ev.OldNick);
                    if (user == null)
                        continue;
                    found = true;
                    channel.Users.Remove(user);
                    channel.Users.Add(new IrcUser(ev.NewNick, user.UserName,
                        user.Host, user.RealName, user.Server));
                    DisplayEvent(ev, channel);
                }
                if (!found)
                {
                    DisplayEvent($"[ Received a NICK message for {ev.OldNick}->{ev.NewNick} who we can't find! ]");
                    return;
                }
            }

        }

        private async Task HandleNoticeEvent(IrcNoticeEvent ev)
        {
            string target = ev.Target;
            if (ev.IsChannelMessage)
            {
                ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                    .FirstOrDefault(c => c.ChannelName == target);
                if (channel == null)
                {
                    DisplayEvent($"[ Received a NOTICE message for {target} which we're not in! ]");
                    return;
                }
                DisplayEvent(ev, channel);
            }
            else
            {
                DisplayEvent(ev);
            }
        }

        private async Task HandlePartEvent(IrcPartEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;
            string nick = ev.User.Nick;
            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                DisplayEvent($"[ Received a PART message for {channelName} which we're not in! ]");
                return;
            }
            if (nick == Nick)
            {
                channel.IsJoined = false;
            }
            else if (channel.Users.All(u => u.Nick != nick))
            {
                DisplayEvent($"[ Received a PART message for {nick} on {channelName} which they weren't in! ]");
                return;
            }
            else
            {
                channel.Users.Remove(channel.Users.First(u => u.Nick == nick));
            }
            DisplayEvent(ev, channel);
        }

        private async Task HandlePingEvent(IrcPingEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            DisplayEvent(ev);
            await SendMessageAsync($"PONG :{ev.Content}");
        }

        private async Task HandlePongEvent(IrcPongEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandlePrivmsgEvent(IrcPrivmsgEvent ev)
        {
            string target = ev.Target;
            if (ev.IsChannelMessage)
            {
                ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                    .FirstOrDefault(c => c.ChannelName == target);
                if (channel == null)
                {
                    DisplayEvent($"[ Received a PRIVMSG message for {target} which we're not in! ]");
                    return;
                }
                DisplayEvent(ev, channel);
            }
            else
            {
                DisplayEvent(ev);
            }
        }

        private async Task HandleQuitEvent(IrcQuitEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string nick = ev.User.Nick;

            bool found = false;
            foreach (ChannelViewModel channel in Channels.Where(c => c.IsJoined))
            {
                IrcUser user = channel.Users.FirstOrDefault(u => u.Nick == nick);
                if (user == null)
                    continue;
                found = true;
                channel.Users.Remove(user);
                DisplayEvent(ev, channel);
            }
            if (!found)
            {
                DisplayEvent($"[ Received a QUIT message for {nick} who we can't find! ]");
                return;
            }
        }

        private async Task HandleServerInfoEvent(IrcServerInfoEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleUnknownCommandEvent(IrcUnknownCommandEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleUserEvent(IrcUserEvent ev)
        {
        }

        private async Task HandleUserModeEvent(IrcUserModeEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;
            DisplayEvent(ev);
        }

        private async Task HandleWelcomeEvent(IrcWelcomeEvent ev)
        {
            Nick = ev.Nick;
            IsConnected = true;
            IsConnecting = false;
            DisplayEvent(ev);

            List<ChannelViewModel> channelsToRejoin = Channels
                .Where(c => c.WasJoinedBeforeDisconnect && !c.IsJoined)
                .ToList();
            if (channelsToRejoin.Count > 0)
            {
                string joinParam = String.Join(",", channelsToRejoin.Select(c => c.ChannelName));
                await SendMessageAsync($"JOIN :{joinParam}");
            }
        }

        private async Task HandleWhoItemEvent(IrcWhoItemEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            IrcUserWhoEntry entry = ev.Entry;
            if (!_whoQueue.ContainsKey(entry.Channel))
                _whoQueue.Add(entry.Channel, new List<IrcUserWhoEntry>());
            _whoQueue[entry.Channel].Add(entry);

            DisplayEvent(ev);
        }

        private async Task HandleWhoEndEvent(IrcWhoEndEvent ev)
        {
            if (ev.EventType != IrcEventType.FromServer) return;

            string channelName = ev.Channel;

            List<IrcUserWhoEntry> entries;
            if (!_whoQueue.TryGetValue(channelName, out entries))
                return;
            _whoQueue[channelName].Clear();

            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
                return;

            foreach (IrcUserWhoEntry entry in entries)
            {
                IrcUser user = channel.Users.FirstOrDefault(u => u.Nick == entry.User.Nick);
                channel.Users.Remove(user);
                channel.Users.Add(entry.User);
            }

            DisplayEvent(ev);
        }

        private async Task HandleYourHostEvent(IrcYourHostEvent ev)
        {
            DisplayEvent(ev);
        }

        private async Task HandleGenericEvent(IrcEvent ev)
        {
            //string output = String.Join(" ", ev.IrcMessage.Parameters.Skip(1));
            //if (!ev.IrcMessage.Trailing.IsNullOrWhitespace())
            //output += $" :{ev.IrcMessage.Trailing}";
            DisplayEvent(ev);
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
