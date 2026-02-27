using System;
using System.Numerics;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.CustomCodeOffsets;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class TargetService(
        IMemoryService memoryService,
        HookManager hookManager,
        IChrInsService chrInsService,
        IReminderService reminderService,
        IPlayerService playerService)
        : ITargetService
    {
        #region Public Methods

        public void ToggleTargetHook(bool isEnabled)
        {
            var code = Base + SaveTargetPtrCode;
            if (isEnabled)
            {
                reminderService.TrySetReminder();
                var saveLoc = Base + LockedTargetPtr;
                var bytes = AsmLoader.GetAsmBytes(AsmScript.SaveTargetPtr);
                AsmHelper.WriteRelativeOffsets(bytes, [
                (code, saveLoc, 7, 3),
                (code + 0xE, Hooks.LastLockedTarget + 7, 5, 0xE + 1)
                ]);
                
                AsmHelper.WriteImmediateDword(bytes, 0x7 + 3, ChrIns.Modules);
                memoryService.WriteBytes(code, bytes);

                byte[] originalBytes = [0x48, 0x8B, 0x80, ..BitConverter.GetBytes(ChrIns.Modules)];
                hookManager.InstallHook(code, Hooks.LastLockedTarget, originalBytes);
            }
            else
            {
                hookManager.UninstallHook(code);
            }
        }

        public nint GetChrIns() =>
            memoryService.Read<nint>(Base + LockedTargetPtr);
        
        public int GetHp() => chrInsService.GetCurrentHp(GetChrIns());

        public int GetMaxHp() => chrInsService.GetMaxHp(GetChrIns());

        public void SetHp(int health) => chrInsService.SetHp(GetChrIns(), health);

        public void ToggleNoDamage(bool isEnabled) => chrInsService.ToggleNoDamage(GetChrIns(), isEnabled);

        public bool IsNoDamageEnabled() => chrInsService.IsNoDamageEnabled(GetChrIns());

        public void SetSpeed(float value) => chrInsService.SetSpeed(GetChrIns(), value);

        public float GetSpeed() => chrInsService.GetSpeed(GetChrIns());

        public Resistances GetResistances() => chrInsService.GetResistances(GetChrIns());

        public Poise GetPoise() => chrInsService.GetPoise(GetChrIns());

        public Immunities GetImmunities() => chrInsService.GetImmunities(GetChrIns());
        
        public Defenses GetDefenses() => chrInsService.GetDefenses(GetChrIns());

        public void ToggleFreezeAi(bool isDisableTargetAiEnabled) =>
            chrInsService.ToggleFreezeAi(GetChrIns(), isDisableTargetAiEnabled);

        public bool IsAiFrozen() => chrInsService.IsFreezeAiEnabled(GetChrIns());

        public Vector3 GetPosition() => chrInsService.GetPosition(GetChrIns());

        public void ToggleRepeatAct(bool isEnabled)
        {
            var aiThink = GetAiThink();
            var forceActPtr = aiThink + ChrIns.AiThinkOffsets.ForceActIdx;
            var lastAct = memoryService.Read<byte>(aiThink + ChrIns.AiThinkOffsets.LastActIdx);
            memoryService.Write(forceActPtr, isEnabled ? lastAct : (byte)0);
        }

        public bool IsRepeatingAct() =>
            memoryService.Read<byte>(GetAiThink() + ChrIns.AiThinkOffsets.ForceActIdx) != 0;

        public void ForceAct(int forceAct) =>
            memoryService.Write(GetAiThink() + ChrIns.AiThinkOffsets.ForceActIdx, (byte)forceAct);

        public int GetLastAct() => memoryService.Read<byte>(GetAiThink() + ChrIns.AiThinkOffsets.LastActIdx);
        public int GetForceAct() => memoryService.Read<byte>(GetAiThink() + ChrIns.AiThinkOffsets.ForceActIdx);
        public int GetCurrentAnimation() => chrInsService.GetCurrentAnimationId(GetChrIns());

        public void ToggleTargetingView(bool isTargetingViewEnabled)
        {
            var targetingView = GetTargetingSystem() + ChrIns.TargetingSystemOffsets.TargetingView;
            memoryService.Write(targetingView, isTargetingViewEnabled ? (byte)6 : (byte)0);
        }

        public bool IsTargetViewEnabled() =>
            memoryService.Read<byte>(GetTargetingSystem() + ChrIns.TargetingSystemOffsets.TargetingView) != 0;

        public void ToggleNoAttack(bool isEnabled) => chrInsService.ToggleNoAttack(GetChrIns(), isEnabled);

        public bool IsNoAttackEnabled() => chrInsService.IsNoAttackEnabled(GetChrIns());

        public void ToggleNoMove(bool isEnabled) => chrInsService.ToggleNoMove(GetChrIns(), isEnabled);

        public bool IsNoMoveEnabled() => chrInsService.IsNoMoveEnabled(GetChrIns());
        public float GetDist() => chrInsService.GetDistBetweenChrs(playerService.GetPlayerIns(), GetChrIns());

        public void ToggleDisableAllExceptTarget(bool isEnabled)
        {
            var code = Base + DisableAllExceptTarget;

            if (isEnabled)
            {
                var bytes = AsmLoader.GetAsmBytes(AsmScript.DisableAllExceptTarget);
                var lockedTarget = Base + LockedTargetPtr;
                AsmHelper.WriteRelativeOffsets(bytes, [
                (code + 0x1, lockedTarget, 7, 0x1 + 3),
                (code + 0x19, Hooks.DisableAllExceptTarget + 5, 5, 0x19 + 1)
                ]);
                
                memoryService.WriteBytes(code, bytes);
                hookManager.InstallHook(code, Hooks.DisableAllExceptTarget, [0x48, 0x89, 0x5C, 0x24, 0x08]);
            }
            else
            {
                hookManager.UninstallHook(code);
            }
        }

        #endregion

        #region Private Methods

        private nint GetAiThink() => memoryService.FollowPointers(GetChrIns(), ChrIns.AiThink, true);

        private nint GetTargetingSystem() => memoryService.FollowPointers(GetChrIns(),
            [..ChrIns.AiThink, ChrIns.AiThinkOffsets.TargetingSystem], true);

        #endregion
    }
}