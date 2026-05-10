//

using System.Numerics;
using SilkySouls3.Models;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Interfaces;

public interface IPlayerService
{
    public nint GetPlayerIns();
    public int GetHp();
    public int GetMaxHp();
    public void SetHp(int hp);
    public int GetMp();
    public int GetMaxMp();
    public void SetMp(int mp);
    public int GetSp();
    public void SetSp(int sp);
    public void ToggleNoDamage(bool isEnabled);
    public float GetPlayerSpeed();
    public void SetPlayerSpeed(float speed);
    public int GetNewGame();
    public void SetNewGame(int value);
    public void ToggleDebugFlag(int offset, int value);
    public void ToggleInfinitePoise(bool isEnabled);
    void ToggleNoHit(bool isEnabled);
    void ToggleInvisible(bool isEnabled);
    void ToggleSilent(bool isEnabled);
    void ToggleInfiniteDurability(bool isEnabled);
    void ToggleNoGoodsConsume(bool isEnabled);
    Stats GetStats();
    int GetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats stat);
    void SetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats stat, int newValue);
    void GiveSouls();
    void ToggleNoRoll(bool isEnabled);
    void Rest();
    Vector3 GetPosition();
    void SavePosition(int index);
    void RestorePosition(int index);
    Position GetCurrentPosition();
    void ForceSetPosition(Vector3 position);
    int GetCurrentBlockId();
    int GetBossGaugeId();
    int GetCurrentAnimationId();
    void BreakWeapon(int slotSelector);
}