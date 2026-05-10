namespace SilkySouls3.Models
{
    public abstract class CustomWarpChange
    {
    }

    public sealed class WarpAdded(CustomWarp warp) : CustomWarpChange
    {
        public CustomWarp Warp { get; } = warp;
    }

    public sealed class WarpDeleted(string category, CustomWarp warp) : CustomWarpChange
    {
        public string Category { get; } = category;
        public CustomWarp Warp { get; } = warp;
    }

    public sealed class CategoryDeleted(string category) : CustomWarpChange
    {
        public string Category { get; } = category;
    }
}
