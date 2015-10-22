using System;
#if DESKTOP
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
#else
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace UnIRC.Shared.Helpers
{

    /// <summary>
    /// This class contains a few useful extenders for the ListBox
    /// </summary>
    public class ListBoxExtenders : DependencyObject
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd",
                typeof(bool), typeof(ListBoxExtenders),
                new PropertyMetadata(default(bool), OnAutoScrollToEndChanged));

        /// <summary>
        /// Returns the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be returned</param>
        /// <returns>The value of the given property</returns>
        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        /// <summary>
        /// Sets the value of the AutoScrollToEndProperty
        /// </summary>
        /// <param name="obj">The dependency-object whichs value should be set</param>
        /// <param name="value">The value which should be assigned to the AutoScrollToEndProperty</param>
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        /// <summary>
        /// This method will be called when the AutoScrollToEnd
        /// property was changed
        /// </summary>
        /// <param name="s">The sender (the ListBox)</param>
        /// <param name="e">Some additional information</param>
        public static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;
            ItemCollection items = listBox?.Items;
            if (items == null || items.Count < 1) return;

            Action<object, object> handlerAction = (s1, e1) =>
            {
                try
                {
                    listBox.UpdateLayout();
                    listBox.ScrollIntoView(items[items.Count - 1]);
                }
                catch { /* nothing to do */ }
            };

#if DESKTOP
            var data = items.SourceCollection as INotifyCollectionChanged;
            if (data == null) throw new InvalidOperationException();
            
            var handler = new NotifyCollectionChangedEventHandler(handlerAction);

            if ((bool)e.NewValue)
                data.CollectionChanged += handler;
            else
                data.CollectionChanged -= handler;
#else
            var handler = new VectorChangedEventHandler<object>(handlerAction);

            if ((bool)e.NewValue)
                items.VectorChanged += handler;
            else
                items.VectorChanged -= handler;
#endif
        }

    }
}
