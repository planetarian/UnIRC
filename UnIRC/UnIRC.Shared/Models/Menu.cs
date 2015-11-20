using System.Collections.Generic;

namespace UnIRC.Models
{
    public class Menu
    {
        public List<MenuItem> MenuItems { get; set; }
        public Menu()
        {
            MenuItems = new List<MenuItem>
            {
                new MenuItem("\uE710", "Connect", "Networks"),
                new MenuItem("\uE81C", "Log", "Log")
            };
            //MenuItems.Add(new MenuItem("\uE1CE", "Menu Item 2", typeof(Page2View)));
            //MenuItems.Add(new MenuItem("\uE1CE", "Menu Item 3", typeof(Page3View)));
            //MenuItems.Add(new MenuItem("\uE1CE", "Menu Item 4", typeof(Page4View)));
        }
    }
}
