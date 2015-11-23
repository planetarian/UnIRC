using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UnIRC.IrcEvents;

namespace UnIRC.Views
{
    public class ConnectionPageTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            Type t = item.GetType();
            const string templateNamePostfix = "ConnectionPageTemplate";
            string templateName = t.Name + templateNamePostfix;
            ResourceDictionary resources = Application.Current.Resources;

            object template;
            if (resources.TryGetValue(templateName, out template) ||
                resources.TryGetValue("IrcEvent" + templateNamePostfix, out template))
                return template as DataTemplate;
            return null;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
