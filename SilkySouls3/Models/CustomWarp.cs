using SilkySouls3.Enums;

namespace SilkySouls3.Models
{
    public class CustomWarp
    {
        public DlcRequirement DlcRequirement { get; set; }
        public string MainArea { get; set; }
        public string Name { get; set; }
        public Position Position { get; set; }
        public int BonfireId { get; set; }
    }
}
