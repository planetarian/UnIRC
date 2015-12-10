using System;
using Windows.System;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
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
        private readonly object _lock = new object();
        private ScrollViewer _messagesScrollViewer;
        private ScrollBar _messagesScrollBar;
        private double _lastVerticalOffset;
        private double _earlyExtentHeight;
        private double _lastExtentHeight;
        private double _lastViewportHeight;
        private double _lastScrollViewerHeight;
        private bool _enterPressed;

        private readonly DispatcherTimer _focusTimer = new DispatcherTimer();

        public ConnectionView()
        {
            InitializeComponent();
        }

        private void ConnectionViewLoaded(object sender, RoutedEventArgs e)
        {
            _focusTimer.Interval = TimeSpan.FromMilliseconds(100);
            _focusTimer.Tick += (s, o) => InputBox.Focus(FocusState.Programmatic);
            _focusTimer.Start();
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
                const double delta = 2;

                double extentHeight = _messagesScrollViewer.ExtentHeight;
                double verticalOffset = _messagesScrollViewer.VerticalOffset;
                double viewportHeight = _messagesScrollViewer.ViewportHeight;
                double scrollViewerHeight = _messagesScrollViewer.ActualHeight;
                double lastDiff = _lastExtentHeight - _lastViewportHeight - _lastVerticalOffset;

                //if (viewportHeight > 0 && viewportFilled && (didNotScrollUp && (extentHeightChanged || scrollViewerHeightChanged)))
                if (delta >= lastDiff && verticalOffset == _lastVerticalOffset)
                {
                    verticalOffset = _messagesScrollViewer.ScrollableHeight;
                    _messagesScrollViewer.ChangeView(null, verticalOffset, null);
                }

                _lastExtentHeight = _earlyExtentHeight;
                _earlyExtentHeight = extentHeight;
                _lastVerticalOffset = verticalOffset;
                _lastViewportHeight = viewportHeight;
                _lastScrollViewerHeight = scrollViewerHeight;

            }
        }
    }
}
