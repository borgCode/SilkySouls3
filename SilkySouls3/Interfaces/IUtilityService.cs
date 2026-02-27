// 

namespace SilkySouls3.Interfaces;

public interface IUtilityService
{
    void Toggle100Drop(bool isEnabled);
    void BreakAllObjects();
    void RestoreAllObjects();
    void SetGameSpeed(float speed);
    float GetGameSpeed();
    void ToggleNoClip(bool isEnabled);
    void WriteNoClipSpeed(float speedScale);
    void ToggleHitboxView(bool isEnabled);
    void ToggleSoundView(bool isEnabled);
    void ToggleGroupMask(int offset, bool isEnabled);
    void ToggleDeathCam(bool isEnabled);
    void SetFov(float fov);
    float GetFov();
    void ToggleCamVertIncrease(bool isEnabled);
    void ToggleFreeCam(bool isEnabled);
    void MoveCamToPlayer();
    void MovePlayerToCam();
    void TogglePlayerMovementForFreeCam(bool isEnabled);
    void ToggleFullLineUp(bool isEnabled);
    void ToggleHitIns(int offset, bool isEnabled);
    void ToggleDbgFps(bool isEnabled);
    void SetFps(float value);
    void ToggleFreezeWorld(bool isEnabled);
}