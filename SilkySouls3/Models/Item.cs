namespace SilkySouls3.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StackSize { get; set; }
        public int UpgradeType { get; set; }
        public bool Infusable { get; set; }
        public string CategoryName { get; set; }
    }
}