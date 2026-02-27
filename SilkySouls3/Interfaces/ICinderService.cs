using SilkySouls3.Enums;

namespace SilkySouls3.Interfaces;

public interface ICinderService
{
    void ForcePhaseTransition(CinderPhase phase);
    void ToggleCinderPhaseLock(bool isEnabled);
    void CastSoulMass();
    void RemoveSoulmass();
    void ToggleEndlessSoulmass(bool isEnabled);
    void ToggleCinderStagger(bool isEnabled);
    void ToggleNoSoulmassRemoveOnStagger(bool isEnabled);
}