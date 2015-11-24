using Windows.System;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using UnIRC.Helpers;
using UnIRC.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UnIRC.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConnectionView : Page
    {
        private object _lock = new object();
        private ScrollViewer _messagesScrollViewer;
        private ScrollBar _messagesScrollBar;
        private double _lastVerticalOffset;
        private double _lastExtentHeight;
        private double _lastViewportHeight;
        private double _lastScrollViewerHeight;
        private bool _enterPressed;

        public ConnectionView()
        {
            InitializeComponent();
        }

        private void MessageBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var vm = DataContext as ConnectionViewModel;
            if (vm == null) return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    if (_enterPressed) return;
                    _enterPressed = true;
                    vm.SendMessageCommand.Execute(null);
                    break;
                case VirtualKey.Up:
                    vm.PrevHistoryMessageCommand.Execute(null);
                    break;
            }
        }

        // VirtualKey.Down can't be handled on KeyDown if it's at position zero
        private void MessageBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            var vm = DataContext as ConnectionViewModel;
            if (vm == null) return;

            switch (e.Key)
            {
                case VirtualKey.Enter:
                    _enterPressed = false;
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

        private void InputLostFocus(object sender, RoutedEventArgs e)
        {
            FocusInputBox();
        }

        private void ServerSelectorClosed(object sender, object e)
        {
            FocusInputBox();
        }

        private void FocusInputBox()
        {
            InputBox.Focus(FocusState.Programmatic);
        }
    }
}
