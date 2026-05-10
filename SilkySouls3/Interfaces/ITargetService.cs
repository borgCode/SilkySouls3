// 

using System.Numerics;
using SilkySouls3.Models;

namespace SilkySouls3.Interfaces;

public interface ITargetService
{
    void ToggleTargetHook(bool isEnabled);
    nint GetChrIns();
    int GetHp();
    int GetMaxHp();
    void SetHp(int health);
    void ToggleNoDamage(bool isEnabled);
    bool IsNoDamageEnabled();
    void SetSpeed(float value);
    float GetSpeed();
    Resistances GetResistances();
    Poise GetPoise();
    Immunities GetImmunities();
    Defenses GetDefenses();
    void ToggleFreezeAi(bool isEnabled);
    bool IsAiFrozen();
    Vector3 GetPosition();
    void ToggleRepeatAct(bool isEnabled);
    public bool IsRepeatingAct();
    public void ForceAct(int forceAct);
    public int GetLastAct();
    public int GetForceAct();
    public int GetCurrentAnimation();
    void ToggleTargetingView(bool isEnabled);
    bool IsTargetViewEnabled();
    void ToggleNoAttack(bool isEnabled);
    bool IsNoAttackEnabled();
    void ToggleNoMove(bool isEnabled);
    bool IsNoMoveEnabled();
    float GetDist();
    void ToggleDisableAllExceptTarget(bool isEnabled);
    void MoveTargetToPlayer();
    int GetMoveTargetStatus();
    void UninstallMoveTargetHook();
    int GetEventId();
}