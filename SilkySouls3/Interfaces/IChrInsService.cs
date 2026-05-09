//

using System.Numerics;
using SilkySouls3.Models;

namespace SilkySouls3.Interfaces;

public interface IChrInsService
{
    void SetHp(nint chrIns, int hp);
    int GetCurrentHp(nint chrIns);
    int GetMaxHp(nint chrIns);
    void SetMp(nint chrIns, int mp);
    int GetMp(nint chrIns);
    int GetMaxMp(nint chrIns);
    void SetSp(nint chrIns, int sp);
    int GetSp(nint chrIns);
    void ToggleNoDamage(nint chrIns, bool isEnabled);
    bool IsNoDamageEnabled(nint chrIns);
    float GetSpeed(nint chrIns);
    void SetSpeed(nint chrIns, float speed);
    void ToggleInfinitePoise(nint chrIns, bool isEnabled);
    bool IsInfinitePoiseEnabled(nint chrIns);
    nint GetChrInsByEntityId(int entityId);
    void RequestEventAnimation(nint chrIns, int animationId);
    int GetCurrentAnimationId(nint chrIns);
    Resistances GetResistances(nint chrIns);
    Poise GetPoise(nint chrIns);
    Immunities GetImmunities(nint chrIns);
    Defenses GetDefenses(nint chrIns);
    Vector3 GetPosition(nint chrIns);
    void SetPosition(nint chrIns, Vector3 position);
    void ToggleFreezeAi(nint chrIns, bool isEnabled);
    bool IsFreezeAiEnabled(nint chrIns);
    void ToggleNoHit(nint chrIns, bool isEnabled);
    bool IsNoHitEnabled(nint chrIns);
    void ToggleNoAttack(nint chrIns, bool isEnabled);
    bool IsNoAttackEnabled(nint chrIns);
    void ToggleNoMove(nint chrIns, bool isEnabled);
    bool IsNoMoveEnabled(nint chrIns);
    void ToggleNoGoodsConsume(nint chrIns, bool isEnabled);
    float GetHitRadius(nint chrIns);
    float GetDistBetweenChrs(nint chrIns1, nint chrIns2);
    void ForceSetPosition(nint chrIns, Vector3 position);
}