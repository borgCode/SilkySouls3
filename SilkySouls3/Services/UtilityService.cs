// 

using System;
using System.Numerics;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class UtilityService(
    IMemoryService memoryService,
    HookManager hookManager,
    IReminderService reminderService,
    IParamService paramService,
    IPlayerService playerService)
    : IUtilityService
{
    private const float DefaultNoClipSpeedScale = 0.2f;
    private const int DefaultLockCamParamRowId = 0;
    private const int FovOffset = 0x14;

    public void Toggle100Drop(bool is100DropEnabled)
    {
        var customCode = CustomCodeOffsets.Base + CustomCodeOffsets.ItemLotBase;

        if (is100DropEnabled)
        {
            reminderService.TrySetReminder();
            var hookLoc = Hooks.ItemLotBase;
            var bytes = AsmLoader.GetAsmBytes(AsmScript.ItemLotBase);
            memoryService.WriteBytes(customCode, bytes);
            hookManager.InstallHook(customCode, hookLoc, [0x45, 0x0F, 0xB7, 0x41, 0x40]);
        }
        else
        {
            hookManager.UninstallHook(customCode);
        }
    }

    public void BreakAllObjects()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.BreakAllObjects);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (memoryService.Read<nint>(WorldObjManImpl.Base), 0x2),
            (Functions.BreakAllObjects, 0x1C + 0x2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    public void RestoreAllObjects()
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.RestoreAllObjects);
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (memoryService.Read<nint>(WorldObjManImpl.Base), 0x2),
            (Functions.RestoreAllObjects, 0x1C + 0x2)
        ]);

        memoryService.AllocateAndExecute(bytes);
    }

    public void SetGameSpeed(float speed) => memoryService.Write(GameSpeed, speed);

    public float GetGameSpeed() => memoryService.Read<float>(GameSpeed);

    public void ToggleNoClip(bool isEnabled)
    {
        var inAirTimerCode = CustomCodeOffsets.Base + CustomCodeOffsets.InAirTimerCode;
        var kbCode = CustomCodeOffsets.Base + CustomCodeOffsets.KeyboardCode;
        var triggersCode = CustomCodeOffsets.Base + CustomCodeOffsets.TriggersCode;
        var updateCoordsCode = CustomCodeOffsets.Base + CustomCodeOffsets.UpdateCoordsCode;

        if (isEnabled)
        {
            WriteInAirTimer(inAirTimerCode);
            WriteKeyboardHook(kbCode);
            WriteRightTriggerCode(triggersCode);
            WriteUpdateCoordsCode(updateCoordsCode);

            hookManager.InstallHook(inAirTimerCode, Hooks.InAirTimer, [0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01, 0x00, 0x00]);
            hookManager.InstallHook(kbCode, Hooks.NoClipKeyboard, [0x41, 0xFF, 0x90, 0xF8, 0x00, 0x00, 0x00]);
            hookManager.InstallHook(triggersCode, Hooks.NoClipTriggers, [0x40, 0x53, 0x57, 0x41, 0x54]);
            hookManager.InstallHook(updateCoordsCode, Hooks.NoClipUpdateCoords,
                [0x66, 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00]);
        }
        else
        {
            hookManager.UninstallHook(inAirTimerCode);
            hookManager.UninstallHook(kbCode);
            hookManager.UninstallHook(triggersCode);
            hookManager.UninstallHook(updateCoordsCode);
        }
    }

    public void WriteNoClipSpeed(float speedScale)
    {
        var ptr = CustomCodeOffsets.Base + CustomCodeOffsets.SpeedScale;
        memoryService.Write(ptr, DefaultNoClipSpeedScale * speedScale);
    }

    public void ToggleHitboxView(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(DamageMan.Base) + DamageMan.HitboxView, isEnabled);

    public void ToggleSoundView(bool isEnabled)
    {
        memoryService.WriteBytes(Patches.DebugFont, [0x90, 0x90, 0x90, 0x90, 0x90]);
        memoryService.Write(Patches.PlayerSoundView + 0x3, isEnabled ? (byte)1 : (byte)0);
    }

    public void ToggleGroupMask(int offset, bool isEnabled) =>
        memoryService.Write(GroupMask.Base + offset, isEnabled ? (byte)0 : (byte)1);

    public void ToggleDeathCam(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(WorldChrManImp.Base) + WorldChrManImp.DeathCam, isEnabled);

    public void SetFov(float fov)
    {
        var row = paramService.GetParamRow((int)Param.LockCamParam, DefaultLockCamParamRowId);
        paramService.Write(row, FovOffset, fov);
    }

    public float GetFov()
    {
        var row = paramService.GetParamRow((int)Param.LockCamParam, DefaultLockCamParamRowId);
        return memoryService.Read<float>(row + FovOffset);
    }

    public void ToggleCamVertIncrease(bool isEnabled)
    {
        var camVertUpHook = Hooks.CameraUpLimit;
        var customCode = CustomCodeOffsets.Base + CustomCodeOffsets.CamVertUp;
        var camVertDown = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), [
            FieldArea.ChrCam,
            FieldArea.ChrExFollowCam,
            FieldArea.CameraDownLimit
        ], false);

        if (isEnabled)
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.CamVertUp);

            var jumpBytes = AsmHelper.GetJmpOriginOffsetBytes(camVertUpHook, 8, customCode + 0x18);
            Array.Copy(jumpBytes, 0, bytes, 0x13 + 1, 4);
            memoryService.WriteBytes(customCode, bytes);
            hookManager.InstallHook(customCode, camVertUpHook, [0xF3, 0x0F, 0x11, 0x86, 0xFC, 0x01, 0x00, 0x00]
            );
            memoryService.Write(camVertDown, 1.5f);
        }
        else
        {
            hookManager.UninstallHook(customCode);
            memoryService.Write(camVertDown, 1.22f);
        }
    }
    
    public void ToggleFreeCam(bool isEnabled)
    {
        var camMode = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), [
            FieldArea.GameRend,
            FieldArea.CamMode
        ], false);
        
        memoryService.Write(camMode, isEnabled ? (byte)1 : (byte)0);
    }

    public void MoveCamToPlayer()
    {
        var freeCamCoordsPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), [
            FieldArea.GameRend,
            FieldArea.DbgFreeCam,
            FieldArea.DbgFreeCamCoords
        ], false);

        var playerPos = playerService.GetPosition();
        playerPos.Y += 2.5f;

        memoryService.Write(freeCamCoordsPtr, playerPos);
    }

    public void MovePlayerToCam()
    {
        var freeCamCoordsPtr = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), [
            FieldArea.GameRend,
            FieldArea.DbgFreeCam,
            FieldArea.DbgFreeCamCoords
        ], false);
        
        playerService.ForceSetPosition(memoryService.Read<Vector4>(freeCamCoordsPtr));
    }

    public void TogglePlayerMovementForFreeCam(bool isEnabled)
    {
        var camMode = memoryService.FollowPointers(memoryService.Read<nint>(FieldArea.Base), [
            FieldArea.GameRend,
            FieldArea.CamMode
        ], false);
        
        memoryService.Write(camMode, isEnabled ? (byte)3 : (byte)1);
    }

    
    public void ToggleFullLineUp(bool isEnabled) =>
        memoryService.WriteBytes(Patches.AccessFullShop, isEnabled
            ? [0x90, 0x90, 0x90, 0x90]
            : [0x84, 0xC0, 0x74, 0x14]);

    public void ToggleHitIns(int offset, bool isEnabled) => memoryService.Write(HitFlags.Base + offset, isEnabled);

    public void ToggleDbgFps(bool isEnabled) =>
        memoryService.Write(memoryService.Read<nint>(SprjFlipper.Base) + SprjFlipper.DebugFpsToggle, isEnabled);

    public void SetFps(float value) =>
        memoryService.Write(memoryService.Read<nint>(SprjFlipper.Base) + SprjFlipper.Fps, value);

    public void ToggleFreezeWorld(bool isEnabled) =>
        memoryService.Write(Patches.IsWorldPaused + 6, isEnabled ? (byte) 1 : (byte) 0);

    #region Private Methods

    private void WriteInAirTimer(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_InAirTimer);
        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrManImp.Base, 7, 0x1 + 3),
            (code + 0x37, Hooks.InAirTimer + 8, 5, 0x37 + 1)
        ]);

        AsmHelper.WriteImmediateDword(bytes, ChrIns.Modules, 0x19 + 3);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteKeyboardHook(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_Keyboard);
        var zDirection = CustomCodeOffsets.Base + CustomCodeOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x17, Hooks.NoClipKeyboard + 7, 5, 0x17 + 1),
            (code + 0x1C, zDirection, 7, 0x1C + 2),
            (code + 0x25, Hooks.NoClipKeyboard + 7, 5, 0x25 + 1),
            (code + 0x2A, zDirection, 7, 0x2A + 2),
            (code + 0x33, Hooks.NoClipKeyboard + 7, 5, 0x33 + 1)
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteRightTriggerCode(nint code)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_Triggers);
        var zDirection = CustomCodeOffsets.Base + CustomCodeOffsets.ZDirection;

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x10, Hooks.NoClipTriggers + 5, 5, 0x10 + 1),
            (code + 0x15, zDirection, 7, 0x15 + 2),
            (code + 0x1D, zDirection, 7, 0x1D + 2),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    private void WriteUpdateCoordsCode(nint code)
    {
        var zDirection = CustomCodeOffsets.Base + CustomCodeOffsets.ZDirection;
        var speedScale = CustomCodeOffsets.Base + CustomCodeOffsets.SpeedScale;

        var bytes = AsmLoader.GetAsmBytes(AsmScript.NoClip_UpdateCoords);

        AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x1, WorldChrManImp.Base, 7, 0x1 + 3),
            (code + 0x67, FD4PadManager.Base, 7, 0x67 + 3),
            (code + 0x70, Functions.GetPad, 5, 0x70 + 1),
            (code + 0x7B, Functions.GetYMovement, 5, 0x7B + 1),
            (code + 0x88, Functions.GetXMovement, 5, 0x88 + 1),
            (code + 0xBC, FieldArea.Base, 7, 0xBC + 3),
            (code + 0xCF, Functions.MatrixTransformVector, 5, 0xCF + 1),
            (code + 0x104, speedScale, 9, 0x104 + 5),
            (code + 0x121, zDirection, 6, 0x121 + 2),
            (code + 0x12F, speedScale, 9, 0x12F + 5),
            (code + 0x158, zDirection, 7, 0x158 + 2),
            (code + 0x18B, Hooks.NoClipUpdateCoords + 8, 5, 0x18B + 1),
        ]);

        memoryService.WriteBytes(code, bytes);
    }

    #endregion
}