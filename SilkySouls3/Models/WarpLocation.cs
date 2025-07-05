namespace SilkySouls3.Models
{
    public class WarpLocation
    {
        public string Name { get; set; }
        public string MainArea { get; set; }
        public int? Offset { get; set; }
        public byte? BitPosition { get; set; }
        public int BonfireId { get; set; }
        public float[] Coords { get; set; }
        public float Angle { get; set; }

        public bool IsStandardWarp => Offset.HasValue && BitPosition.HasValue;
        public bool HasCoordinates => Coords != null && Coords.Length > 0;
    }
}