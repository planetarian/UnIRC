using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace UnIRC.Helpers
{
    public static class ControlExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DependencyObject GetChildElement(this DependencyObject reference, params int[] indices)
        {
            return indices.Aggregate(reference, VisualTreeHelper.GetChild);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetChildrenCount(this DependencyObject reference)
        {
            return VisualTreeHelper.GetChildrenCount(reference);
        }
    }
}
