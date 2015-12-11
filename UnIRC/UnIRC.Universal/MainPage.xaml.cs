using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.ExtendedExecution.Foreground;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using UnIRC.Models;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using GalaSoft.MvvmLight.Messaging;
using UnIRC.Annotations;
using UnIRC.Shared.Messages;
using UnIRC.ViewModels;
using UnIRC.Views;

namespace UnIRC
{
    public sealed partial class MainPage
    {
        public static readonly DependencyProperty IsConnectionSelectedProperty = DependencyProperty.Register(
            "IsConnectionSelected", typeof (bool), typeof (MainPage), new PropertyMetadata(default(bool)));

        public bool IsConnectionSelected
        {
            get { return (bool) GetValue(IsConnectionSelectedProperty); }
            set { SetValue(IsConnectionSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsChannelSelectedProperty = DependencyProperty.Register(
            "IsChannelSelected", typeof (bool), typeof (MainPage), new PropertyMetadata(default(bool)));

        public bool IsChannelSelected
        {
            get { return (bool) GetValue(IsChannelSelectedProperty); }
            set { SetValue(IsChannelSelectedProperty, value); }
        }

        private readonly Dictionary<string, Type> _views
            = new Dictionary<string, Type>
            {
                {"Networks", typeof (NetworksView)},
                {"Log", typeof (LogView)}
            };
        

        private readonly ListBox[] _allMenus;
        private readonly ListBox[] _fixedMenus;

        private readonly Dictionary<ConnectionViewModel, Page> _connectionPages
            = new Dictionary<ConnectionViewModel, Page>();

        private readonly Dictionary<ConnectionViewModel, Dictionary<ChannelViewModel, Page>> _channelPages
            = new Dictionary<ConnectionViewModel, Dictionary<ChannelViewModel, Page>>();

        public MainPage()
        {
            InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(340, 530));
            Application.Current.Suspending += OnSuspending;

            _allMenus = new[] {UpperFixedMenu, ConnectionsMenu, ChannelsMenu, LowerFixedMenu};
            _fixedMenus = new[] {UpperFixedMenu, LowerFixedMenu};

        }
        private async void OnSuspending(object o, SuspendingEventArgs ev)
        {
            Messenger.Default.Send(new ErrorMessage("Attempting to extend suspension."));
            SuspendingDeferral deferral = ev.SuspendingOperation.GetDeferral();
            using (var session = new ExtendedExecutionForegroundSession
            {Reason = ExtendedExecutionForegroundReason.Unconstrained})
            {
                session.Description = "Extend IRC session";
                session.Revoked += (s, e) => { SuspendAll(); };
                try
                {
                    if (await session.RequestExtensionAsync() == ExtendedExecutionForegroundResult.Denied)
                        SuspendAll();
                }
                catch (Exception ex)
                {
                    Messenger.Default.Send(new ErrorMessage("Exception while deferring suspension.", ex));
                }
            }


        }

        private void SuspendAll()
        {
            Messenger.Default.Send(new ErrorMessage("Suspending."));

            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            foreach (ConnectionViewModel cvm in vm.Connections)
                cvm.SuspendCommand.Execute(null);
        }

        private void ConnectionsChanged(IObservableVector<object> sender, IVectorChangedEventArgs ev)
        {
            /*
            var index = (int)ev.Index;
            switch (ev.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                    object changedElement = sender[index];
                    var vm = changedElement as ConnectionViewModel;
                    if (vm == null) return;

                    var view = new ConnectionView {DataContext = vm};
                    _connectionPages.Add(view);
                    // Open the new connection view
                    ConnectionsMenu.SelectedItem = vm;
                    break;
                case CollectionChange.ItemRemoved:
                    _connectionPages.RemoveAt(index);
                    break;
            }//*/
        }

        private void MenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            ConnectionViewModel selectedConnection = vm.MenuSelectedConnection;
            ChannelViewModel selectedChannel = selectedConnection?.SelectedChannel;

            var list = (ListBox) sender;
            if (list.SelectedIndex == -1)
                return;

            foreach (ListBox listBox in _allMenus.Where(listBox => listBox != list))
            {
                if (list != ChannelsMenu || (list == ChannelsMenu && listBox != ConnectionsMenu))
                listBox.SelectedItem = null;
            }

            IsConnectionSelected = false;
            IsChannelSelected = false;

            if (list == ConnectionsMenu)
            {
                if (_connectionPages.ContainsKey(selectedConnection))
                {
                    ContentFrame.Content = _connectionPages[selectedConnection];
                    IsConnectionSelected = true;
                }
            }
            else if (list == ChannelsMenu)
            {
                if (!_channelPages[selectedConnection].ContainsKey(selectedChannel))
                {
                    var view = new ChannelView { DataContext = selectedChannel };
                    _channelPages[selectedConnection].Add(selectedChannel, view);
                }
                ContentFrame.Content = _channelPages[selectedConnection][selectedChannel];
                IsChannelSelected = true;
            }
            else if (_fixedMenus.Contains(list))
            {
                string key = (list.SelectedItem as MenuItem)?.ViewKey;
                if (_views.ContainsKey(key))
                    ContentFrame.Navigate(_views[key]);
            }

            if (NavigationSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay
                || NavigationSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                NavigationSplitView.IsPaneOpen = false;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationSplitView.IsPaneOpen = !NavigationSplitView.IsPaneOpen;
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            //if (ConnectionsMenu.Items != null)
                //ConnectionsMenu.Items.VectorChanged += ConnectionsChanged;

            var vm = DataContext as MainViewModel;
            if (vm != null)
                vm.Connections.CollectionChanged += ConnectionsChanged;
        }

        private void ConnectionsChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;

            ConnectionViewModel[] oldItems = (ev.OldItems??new ArrayList()).Cast<ConnectionViewModel>().ToArray();
            ConnectionViewModel[] newItems = (ev.NewItems??new ArrayList()).Cast<ConnectionViewModel>().ToArray();

            IEnumerable<ConnectionViewModel> added = newItems.Where(i => !oldItems.Contains(i));
            IEnumerable<ConnectionViewModel> removed = oldItems.Where(i => !newItems.Contains(i));

            foreach (ConnectionViewModel connection in removed)
            {
                if (_channelPages.ContainsKey(connection))
                {
                    _channelPages[connection].Clear();
                    _channelPages.Remove(connection);
                }
                _connectionPages.Remove(connection);
                //connection.Channels.CollectionChanged -= ChannelsChanged;
            }
            foreach (ConnectionViewModel connection in added)
            {
                var view = new ConnectionView { DataContext = connection };
                _connectionPages.Add(connection, view);
                _channelPages.Add(connection, new Dictionary<ChannelViewModel, Page>());
                vm.SelectedConnection = connection;
                //connection.Channels.CollectionChanged += ChannelsChanged;
            }
        }

        private void ChannelsChanged(object sender, NotifyCollectionChangedEventArgs ev)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            ConnectionViewModel connection = vm.MenuSelectedConnection;
            
            if (!_channelPages.ContainsKey(connection))
                throw new Exception("Connection not found in page cache");

            ChannelViewModel[] oldItems = (ev.OldItems ?? new ArrayList()).Cast<ChannelViewModel>().ToArray();
            ChannelViewModel[] newItems = (ev.NewItems ?? new ArrayList()).Cast<ChannelViewModel>().ToArray();

            IEnumerable<ChannelViewModel> added = newItems.Where(i => !oldItems.Contains(i));
            IEnumerable<ChannelViewModel> removed = oldItems.Where(i => !newItems.Contains(i));


            Dictionary<ChannelViewModel, Page> pages = _channelPages[connection];
            foreach (ChannelViewModel channel in removed.Where(channel => pages.ContainsKey(channel)))
            {
                pages.Remove(channel);
            }
            foreach (ChannelViewModel channel in added)
            {
                var view = new ChannelView { DataContext = channel };
                pages.Add(channel, view);
                connection.SelectedChannel = channel;
            }
        }

        private void ChannelsMenuTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            ConnectionViewModel selectedConnection = vm.MenuSelectedConnection;
            ChannelViewModel selectedChannel = selectedConnection?.SelectedChannel;

            if (IsChannelSelected || selectedConnection == null || selectedChannel == null) return;

            ContentFrame.Content = _channelPages[selectedConnection][selectedChannel];
            IsConnectionSelected = false;
            IsChannelSelected = true;
        }

        private void NetworksMenuTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            ConnectionViewModel selectedConnection = vm.MenuSelectedConnection;

            if (IsConnectionSelected || selectedConnection == null) return;

            ContentFrame.Content = _connectionPages[selectedConnection];
            selectedConnection.SelectedChannel = null;
            IsChannelSelected = false;
            IsConnectionSelected = true;
        }
    }
}
