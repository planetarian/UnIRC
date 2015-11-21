using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using UnIRC.Models;
using Windows.UI.Xaml.Controls;
using UnIRC.Shared.ViewModels;
using UnIRC.Views;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnIRC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Dictionary<string, Type> _views
            = new Dictionary<string, Type>
            {
                {"Networks", typeof (NetworksView)},
                {"Log", typeof (LogView)}
            };
        

        private readonly ListBox[] _allMenus;
        private readonly ListBox[] _fixedMenus;

        private readonly List<Page> _connectionPages = new List<Page>();

        public MainPage()
        {
            InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(340, 530));
            _allMenus = new[] {UpperFixedMenu, ConnectionsMenu, LowerFixedMenu};
            _fixedMenus = new[] {UpperFixedMenu, LowerFixedMenu};
            if (ConnectionsMenu.Items != null)
                ConnectionsMenu.Items.VectorChanged += ConnectionsChanged;
        }

        private void ConnectionsChanged(IObservableVector<object> sender, IVectorChangedEventArgs ev)
        {
            var index = (int)ev.Index;
            if (ev.CollectionChange == CollectionChange.ItemInserted)
            {
                object changedElement = sender[index];
                var vm = changedElement as ConnectionViewModel;
                if (vm != null)
                    _connectionPages.Add(new ConnectionView {DataContext = vm});
            }
            else if (ev.CollectionChange == CollectionChange.ItemRemoved)
            {
                _connectionPages.RemoveAt(index);
            }

        }

        private void MenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            if (list.SelectedIndex == -1)
                return;

            foreach (ListBox listBox in _allMenus.Where(listBox => listBox != list))
                listBox.SelectedItem = null;

            if (list == ConnectionsMenu)
            {
                int index = ConnectionsMenu.SelectedIndex;
                if (_connectionPages.Count > index)
                    ContentFrame.Content = _connectionPages[index];
            }
            else if (_fixedMenus.Contains(list))
            {
                string key = (list.SelectedItem as MenuItem)?.ViewKey;
                if (_views.ContainsKey(key))
                    ContentFrame.Navigate(_views[key]);
            }

            if (NavigationSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || NavigationSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                NavigationSplitView.IsPaneOpen = false;
        }

        private void MenuButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NavigationSplitView.IsPaneOpen = !NavigationSplitView.IsPaneOpen;
        }
    }
}
