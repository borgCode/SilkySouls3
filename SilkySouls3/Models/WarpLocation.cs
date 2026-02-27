using System.Numerics;
using SilkySouls3.Enums;

namespace SilkySouls3.Models
{
    public class WarpLocation
    {
        public DlcRequirement DlcRequirement { get; set; }
        public string Name { get; set; }
        public string MainArea { get; set; }
        public int? Offset { get; set; }
        public byte? BitPosition { get; set; }
        public int BonfireId { get; set; }
        public Vector3? Coords { get; set; }
        public float Angle { get; set; }

        public bool IsStandardWarp => Offset.HasValue && BitPosition.HasValue;
        public bool HasCoordinates => Coords.HasValue;
    }
}
