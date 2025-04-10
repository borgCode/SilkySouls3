namespace SilkySouls3.Models
{
    public class WarpEntry
    {
        public string Name { get; set; }
        public int? Offset { get; set; }
        public byte? BitPosition { get; set; }
        public int BonfireID { get; set; }
        public float[] Coords { get; set; }
        public float Angle { get; set; }

        public bool IsStandardWarp => Offset.HasValue && BitPosition.HasValue;
        public bool HasCoordinates => Coords != null && Coords.Length > 0;
    }
}