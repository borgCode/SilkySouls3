//

namespace SilkySouls3.Models;

public class SpEffectEntry(int id, float timeLeft, float duration, ushort stateInfo)
{
    public int Id { get; set; } = id;
    public float TimeLeft { get; set; } = timeLeft;
    public float Duration { get; set; } = duration;
    public ushort StateInfo { get; set; } = stateInfo;
}
