using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Ioc;
using UnIRC.IrcEvents;
using UnIRC.Models;
using UnIRC.Shared.Helpers;

namespace UnIRC.ViewModels
{
    public class ChannelViewModel : ViewModelBaseExtended
    {
        public ConnectionViewModel Connection
        {
            get { return _connection; }
            set { Set(ref _connection, value); }
        }
        private ConnectionViewModel _connection;


        public string ChannelName
        {
            get { return _channelName; }
            set { Set(ref _channelName, value); }
        }
        private string _channelName;
        

        public bool IsJoined
        {
            get { return _isJoined; }
            set { Set(ref _isJoined, value); }
        }
        private bool _isJoined;

        public bool WasJoinedBeforeDisconnect
        {
            get { return _wasJoinedBeforeDisconnect; }
            set { Set(ref _wasJoinedBeforeDisconnect, value); }
        }
        private bool _wasJoinedBeforeDisconnect;


        public ObservableCollection<IrcUser> Users
        {
            get { return _users; }
            set { Set(ref _users, value); }
        }
        private ObservableCollection<IrcUser> _users
            = new ObservableCollection<IrcUser>();


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

        public bool IsActive
        {
            get { return _isActive; }
            set { Set(ref _isActive, value); }
        }
        private bool _isActive;

        public bool SendSlashMessageToChannel
        {
            get { return _sendSlashMessageToChannel; }
            set { Set(ref _sendSlashMessageToChannel, value); }
        }
        private bool _sendSlashMessageToChannel;



        public ICommand SendMessageCommand { get; set; }
        public ICommand PrevHistoryMessageCommand { get; set; }
        public ICommand NextHistoryMessageCommand { get; set; }

        public ChannelViewModel()
        {
        }

        public ChannelViewModel(ConnectionViewModel connection,
            string channelName, IrcUser user, bool isJoined = true)
        {
            Connection = connection;
            ChannelName = channelName;
            Users.Add(user);
            IsJoined = isJoined;

            SendMessageCommand = GetCommand(async () => await SendMessageToChannelAsync());

            PrevHistoryMessageCommand = GetCommand(PrevHistoryMessage,
                () => CurrentMessageHistoryIndex > 0,
                () => CurrentMessageHistoryIndex);
            NextHistoryMessageCommand = GetCommand(NextHistoryMessage,
                () => !InputMessage.IsNullOrEmpty() && CurrentMessageHistoryIndex <= MessagesSent.Count,
                () => CurrentMessageHistoryIndex);
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
            }
        }

        private async Task SendMessageToChannelAsync()
        {
            bool sendSlashMessage = SendSlashMessageToChannel;
            string inputMessage = InputMessage;

            if (inputMessage.IsNullOrWhitespace() || inputMessage == "/")
                return;

            InputMessage = "";
            MessagesSent.Add(inputMessage);
            CurrentMessageHistoryIndex = MessagesSent.Count;
            
            if (sendSlashMessage || !inputMessage.StartsWith("/"))
                await Connection.SendMessageAsync($"PRIVMSG {ChannelName} :{inputMessage}");
            else if (inputMessage.StartsWith("/me "))
                await Connection.SendMessageAsync(
                    $"PRIVMSG {ChannelName} :\x0001ACTION {inputMessage.Substring(4)}\x0001");
            else // slash messages to be sent as raw commands
                await Connection.SendMessageAsync(inputMessage.Substring(1));
            
        }
    }
}
