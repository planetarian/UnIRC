namespace UnIRC.Models
{
    public class MenuItem
    {
        public string Icon { get; private set; }
        public string Title { get; private set; }
        public string ViewKey { get; private set; }

        public MenuItem(string icon, string title, string viewKey)
        {
            Icon = icon;
            Title = title;
            ViewKey = viewKey;
        }
    }
}
