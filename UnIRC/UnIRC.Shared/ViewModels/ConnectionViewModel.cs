using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Networking;
using Windows.UI.Core;
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


        private readonly Dictionary<string, List<IrcUserNamesEntry>> _namesQueue
            = new Dictionary<string, List<IrcUserNamesEntry>>();
        private readonly Dictionary<string, List<IrcUserWhoEntry>> _whoQueue
            = new Dictionary<string, List<IrcUserWhoEntry>>();

        private readonly SemaphoreSlim _eventProcessingLock = new SemaphoreSlim(1);

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
                Servers = Network.Servers.ToObservable();
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
            _namesQueue.Clear();
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
                    await HandleIrcEventAsync(ev);
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
                    incomingMessage = $"[ WaitForData() Error: {ex.Message} ]{Environment.NewLine}{ex}";
                }
                DisplayEvent(incomingMessage);
            }
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

        private async Task HandleIrcEventAsync(IrcEvent ev)
        {
            DisplayEvent(ev);
            using (await _eventProcessingLock.LockAsync())
            {
                await new TypeSwitch()
                    .Case<IrcEvent>(e => { })
                    .Case<IrcChannelModeEvent>(HandleEvent)
                    .Case<IrcInviteEvent>(HandleEvent)
                    .Case<IrcJoinEvent>(HandleEvent)
                    .Case<IrcKickEvent>(HandleEvent)
                    .Case<IrcNickEvent>(HandleEvent)
                    .Case<IrcNamesItemEvent>(HandleEvent)
                    .Case<IrcNamesEndEvent>(HandleEvent)
                    .Case<IrcNoticeEvent>(HandleEvent)
                    .Case<IrcPartEvent>(HandleEvent)
                    .Case<IrcPingEvent>(HandleEvent)
                    .Case<IrcPrivmsgEvent>(HandleEvent)
                    .Case<IrcQuitEvent>(HandleEvent)
                    .Case<IrcUserModeEvent>(HandleEvent)
                    .Case<IrcWhoItemEvent>(HandleEvent)
                    .Case<IrcWhoEndEvent>(HandleEvent)
                    .SwitchAsync(ev);
            }
        }

        private async Task HandleEvent(IrcChannelModeEvent ev)
        {
            string channelName = ev.Channel;
            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                DisplayEvent("[ Received a MODE message for a channel we're not in! ]");
                return;
            }
            channel.Messages.Add(ev);
        }

        private async Task HandleEvent(IrcInviteEvent ev)
        {
        }

        private async Task HandleEvent(IrcJoinEvent ev)
        {
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
            channel.Messages.Add(ev);
        }

        private async Task HandleEvent(IrcKickEvent ev)
        {
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
            channel.Messages.Add(ev);
        }

        private async Task HandleEvent(IrcNamesItemEvent ev)
        {
            if (!_namesQueue.ContainsKey(ev.Channel))
                _namesQueue.Add(ev.Channel, new List<IrcUserNamesEntry>());
            _namesQueue[ev.Channel].AddRange(ev.Entries);
        }

        private async Task HandleEvent(IrcNamesEndEvent ev)
        {
            string channelName = ev.Channel;

            List<IrcUserNamesEntry> entries;
            if (!_namesQueue.TryGetValue(channelName, out entries))
                return;
            _namesQueue[channelName].Clear();

            ChannelViewModel channel = Channels.Where(c => c.IsJoined)
                .FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
                return;

            // Remove users that have left
            foreach (IrcUser user in channel.Users
                .Where(u => entries.All(e => e.Nick != u.Nick)))
                channel.Users.Remove(user);
            // Add users who have joined
            foreach (IrcUserNamesEntry entry in entries
                .Where(e => channel.Users.All(u => u.Nick != e.Nick)))
                channel.Users.Add(new IrcUser(entry.Nick));

            channel.Messages.Add(ev);
        }

        private async Task HandleEvent(IrcNickEvent ev)
        {
            if (ev.OldNick == Nick)
            {
                Nick = ev.NewNick;
                foreach (ChannelViewModel channel in Channels.Where(c => c.IsJoined))
                {
                    IrcUser user = channel.Users.First(u => u.Nick == ev.OldNick);
                    channel.Users.Remove(user);
                    channel.Users.Add(new IrcUser(ev.NewNick, user.UserName, user.Host, user.RealName, user.Server));
                    channel.Messages.Add(ev);
                }
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
                    channel.Messages.Add(ev);
                }
                if (!found)
                {
                    DisplayEvent($"[ Received a NICK message for {ev.OldNick}->{ev.NewNick} who we can't find! ]");
                    return;
                }
            }
        }

        private async Task HandleEvent(IrcNoticeEvent ev)
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
                channel.Messages.Add(ev);
            }
        }

        private async Task HandleEvent(IrcPartEvent ev)
        {
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
            channel.Messages.Add(ev);
        }

        private async Task HandleEvent(IrcPingEvent ev)
        {
            await SendMessageAsync($"PONG :{ev.Content}");
        }

        private async Task HandleEvent(IrcPrivmsgEvent ev)
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
                channel.Messages.Add(ev);
            }
        }

        private async Task HandleEvent(IrcQuitEvent ev)
        {
            string nick = ev.User.Nick;

            bool found = false;
            foreach (ChannelViewModel channel in Channels.Where(c => c.IsJoined))
            {
                IrcUser user = channel.Users.FirstOrDefault(u => u.Nick == nick);
                if (user == null)
                    continue;
                found = true;
                channel.Users.Remove(user);
                channel.Messages.Add(ev);
            }
            if (!found)
            {
                DisplayEvent($"[ Received a QUIT message for {nick} who we can't find! ]");
                return;
            }
        }

        private async Task HandleEvent(IrcUserModeEvent ev)
        {
        }

        private async Task HandleEvent(IrcWhoItemEvent ev)
        {
            IrcUserWhoEntry entry = ev.Entry;
            if (!_whoQueue.ContainsKey(entry.Channel))
                _whoQueue.Add(entry.Channel, new List<IrcUserWhoEntry>());
            _whoQueue[entry.Channel].Add(entry);
        }

        private async Task HandleEvent(IrcWhoEndEvent ev)
        {
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
                var user = channel.Users.FirstOrDefault(u => u.Nick == entry.User.Nick);
                channel.Users.Remove(user);
                channel.Users.Add(entry.User);
            }

            channel.Messages.Add(ev);
        }
    }
}
