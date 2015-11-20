using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using UnIRC.Models;
using Windows.UI.Xaml.Controls;
using UnIRC.Views;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnIRC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Dictionary<string, Type> Views
            = new Dictionary<string, Type>
            {
                {"Networks", typeof (NetworksView)},
                {"Log", typeof (LogView)}
            };

        public MainPage()
        {
            InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(340, 530));
        }

        private void Menu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            if (list.SelectedIndex == -1)
                return;

            string key = (list.SelectedItem as MenuItem)?.ViewKey;
            if (Views.ContainsKey(key))
                ContentFrame.Navigate(Views[key]);

            if (NavigationSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || NavigationSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                NavigationSplitView.IsPaneOpen = false;
        }

        private void MenuButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            NavigationSplitView.IsPaneOpen = !NavigationSplitView.IsPaneOpen;
        }
    }
}
