using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
#endif

namespace UnIRC.Shared.Helpers
{
    public static class InlinesBinding
    {
        public static string GetInlineList(TextBlock element)
        {
            if (element != null)
                return element.GetValue(InlineListProperty) as string;
            return string.Empty;
        }

        public static void SetInlineList(TextBlock element, string value)
        {
            element?.SetValue(InlineListProperty, value);
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.RegisterAttached(
                "InlineList",
                typeof(List<Inline>),
                typeof(InlinesBinding),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBlock;
            if (tb == null) return;

            // clear previous inlines
            tb.Inlines.Clear();

            // add new inlines
            var inlines = e.NewValue as List<Inline>;
            inlines?.ForEach(inl => tb.Inlines.Add(inl));
        }

    }
}
