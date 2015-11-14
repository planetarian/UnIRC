using System;

namespace UnIRC.Models
{
    public class MenuItem
    {
        public string Icon { get; private set; }
        public string Title { get; private set; }
        public Type View { get; private set; }

        public MenuItem(string icon, string title, Type view)
        {
            Icon = icon;
            Title = title;
            View = view;
        }
    }
}
