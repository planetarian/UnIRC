using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UnIRC.Helpers;
using UnIRC.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UnIRC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelView : Page
    {
        private object _lock = new object();
        private ScrollViewer _messagesScrollViewer;
        private ScrollBar _messagesScrollBar;
        private double _lastVerticalOffset;
        private double _lastExtentHeight;
        private double _lastViewportHeight;
        private double _lastScrollViewerHeight;

        private bool _ctrlPressed;
        private bool _enterPressed;

        private bool _autoRefocus;

        public ChannelView()
        {
            InitializeComponent();
        }

        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _autoRefocus = e.Pointer.PointerDeviceType != PointerDeviceType.Touch;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            FocusInput();
        }

        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            FocusInput();
        }

        private void DropDownClosed(object sender, object e)
        {
            FocusInput();
        }

        private void FocusInput()
        {
            if (_autoRefocus)
                InputBox.Focus(FocusState.Programmatic);
        }

        private void MessageBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var vm = DataContext as ChannelViewModel;
            if (vm == null) return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    if (_enterPressed) return;
                    _enterPressed = true;
                    vm.SendMessageCommand.Execute(null);
                    break;
                case VirtualKey.Control:
                    if (_ctrlPressed) return;
                    _ctrlPressed = true;
                    vm.SendSlashMessageToChannel = true;
                    break;
                case VirtualKey.Up:
                    vm.PrevHistoryMessageCommand.Execute(null);
                    break;
            }
        }

        // VirtualKey.Down can't be handled on KeyDown if it's at position zero
        private void MessageBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            var vm = DataContext as ChannelViewModel;
            if (vm == null) return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    _enterPressed = false;
                    break;
                case VirtualKey.Control:
                    if (!_ctrlPressed) return;
                    _ctrlPressed = false;
                    vm.SendSlashMessageToChannel = false;
                    break;
                case VirtualKey.Down:
                    vm.NextHistoryMessageCommand.Execute(null);
                    break;
            }
        }

        private void MessagesLoaded(object sender, RoutedEventArgs ev)
        {
            if (Messages.Items == null) return;

            var listBorder = VisualTreeHelper.GetChild(Messages, 0) as Border;
            _messagesScrollViewer = VisualTreeHelper.GetChild(listBorder, 0) as ScrollViewer;
            _messagesScrollBar = _messagesScrollViewer.GetChildElement(0, 0, 1) as ScrollBar;
            _messagesScrollBar.LayoutUpdated += (o, e) => DoScrollCheck();
        }

        private void DoScrollCheck()
        {
            lock (_lock)
            {
                // VerticalOffset when fully scrolled up is 2, so the delta must be greater than that.
                const double delta = 3;

                double extentHeight = _messagesScrollViewer.ExtentHeight;
                double verticalOffset = _messagesScrollViewer.VerticalOffset;
                double viewportHeight = _messagesScrollViewer.ViewportHeight;
                double scrollViewerHeight = _messagesScrollViewer.ActualHeight;

                bool viewportFilled = extentHeight > viewportHeight;
                bool extentHeightChanged = extentHeight != _lastExtentHeight;
                bool scrollViewerHeightChanged = scrollViewerHeight != _lastScrollViewerHeight;
                bool didNotScrollUp = _lastVerticalOffset > (_lastExtentHeight - _lastViewportHeight) - delta;

                if (viewportFilled && (didNotScrollUp && (extentHeightChanged || scrollViewerHeightChanged)))
                {
                    verticalOffset = _messagesScrollViewer.ScrollableHeight;
                    _messagesScrollViewer.ChangeView(null, verticalOffset, null);
                }

                _lastExtentHeight = extentHeight;
                _lastVerticalOffset = verticalOffset;
                _lastViewportHeight = viewportHeight;
                _lastScrollViewerHeight = scrollViewerHeight;

            }
        }
    }
}
