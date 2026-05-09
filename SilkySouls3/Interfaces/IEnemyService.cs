// 

namespace SilkySouls3.Interfaces;

public interface IEnemyService
{
    void ToggleDebugFlag(int offset, bool isEnabled);
    void ToggleAllRepeatAct(bool isEnabled);
    void ToggleTargetingView(bool isTargetingViewEnabled);
    void PlacePrismStones();
    void ToggleButterflyRng(bool isEnabled);
    void SetLeftButterflyAttack(float animationId);
    void SetRightButterflyAttack(float animationId);
    void TogglePontiffNoClone(bool isEnabled);
    void ToggleDrawNavigation(bool isEnabled);
}