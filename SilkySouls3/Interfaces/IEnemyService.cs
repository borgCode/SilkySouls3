// 

namespace SilkySouls3.Interfaces;

public interface IEnemyService
{
    void ToggleDebugFlag(int offset, bool isEnabled);
    void ToggleAllRepeatAct(bool isEnabled);
    void ToggleTargetingView(bool isTargetingViewEnabled);
    void PlacePrismStones();
}