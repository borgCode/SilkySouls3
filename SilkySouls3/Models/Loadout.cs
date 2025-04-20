using System.Collections.Generic;

namespace SilkySouls3.Models
{
    public class ItemTemplate
    {
        public string ItemName { get; set; }
        public string Infusion { get; set; } = "Normal";
        public int Upgrade { get; set; }
    }

    public class LoadoutTemplate
    {
        public string Name { get; set; }
        public List<ItemTemplate> Items { get; set; }
    }

    public static class LoadoutTemplates
    {
        public static readonly LoadoutTemplate Sl1 = new LoadoutTemplate
        {
            Name = "SL1",
            Items = new List<ItemTemplate>
            {
                new ItemTemplate { ItemName = "Club", Infusion = "Raw", Upgrade = 10 },
                new ItemTemplate { ItemName = "Club"},
                new ItemTemplate { ItemName = "Broadsword", Infusion = "Raw", Upgrade = 10 },
                new ItemTemplate { ItemName = "Broadsword",},
                new ItemTemplate { ItemName = "Dagger", Infusion = "Raw", Upgrade = 10 },
                new ItemTemplate { ItemName = "Dagger"},
                new ItemTemplate { ItemName = "Bandit's Knife", Infusion = "Raw", Upgrade = 10 },
                new ItemTemplate { ItemName = "Bandit's Knife"},
                new ItemTemplate { ItemName = "Sorcerer's Staff"},
                new ItemTemplate { ItemName = "Spook"},
                new ItemTemplate { ItemName = "Hidden Body"},
                new ItemTemplate { ItemName = "Small Leather Shield"},
                new ItemTemplate { ItemName = "Storyteller's Staff" },
                new ItemTemplate { ItemName = "Chloranthy Ring" },
                new ItemTemplate { ItemName = "Chloranthy Ring+3" },
                new ItemTemplate { ItemName = "Scholar Ring" },
                new ItemTemplate { ItemName = "Priestess Ring" },
                new ItemTemplate { ItemName = "Red Tearstone Ring" },
                new ItemTemplate { ItemName = "Hunter's Ring" },
                new ItemTemplate { ItemName = "Knight's Ring" },
                new ItemTemplate { ItemName = "Carthus Milkring" },
                new ItemTemplate { ItemName = "Flynn's Ring" },
                new ItemTemplate { ItemName = "Prisoner's Chain" }
            }
        };

        public static readonly List<LoadoutTemplate> All = new List<LoadoutTemplate> { Sl1 };
    }

}