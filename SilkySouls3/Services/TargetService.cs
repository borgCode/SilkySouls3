using System;
using System.Numerics;
using System.Runtime.InteropServices;
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
        
        
        private static readonly (float dx, float dz)[] CastShapeDirs =
        {
            ( 1f,  0f), (-1f,  0f), ( 0f,  1f), ( 0f, -1f),
            ( 0.7071f,  0.7071f), ( 0.7071f, -0.7071f),
            (-0.7071f,  0.7071f), (-0.7071f, -0.7071f),
        };
        
        private static readonly float[] CastShapeRadii = [2f, 3f, 4f];

        private const int CastShapeCandidateCount = 25;
        private const float CastShapeSweepRadius = 0.35f;
        private const float CastShapeProbeYOffset = 1.0f; 
        
        
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
                
                AsmHelper.WriteImmediateDword(bytes, ChrIns.Modules, 0x7 + 3);
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

        public void MoveTargetToPlayer()
        {
            var playerPos = playerService.GetPosition();
            var targetPos = GetPosition();
            WriteCastShapeCandidates(playerPos, targetPos);
            InstallMoveTargetHook(playerPos);
        }

        public int GetMoveTargetStatus() =>
            memoryService.Read<int>(Base + (int)MoveTarget.Status);

        public void UninstallMoveTargetHook() =>
            hookManager.UninstallHook(Base + (int)MoveTarget.Code);

        public int GetEventId() => chrInsService.GetEventId(GetChrIns());

        #endregion

        #region Private Methods

        private nint GetAiThink() => memoryService.FollowPointers(GetChrIns(), ChrIns.AiThink, true);

        private nint GetTargetingSystem() => memoryService.FollowPointers(GetChrIns(),
            [..ChrIns.AiThink, ChrIns.AiThinkOffsets.TargetingSystem], true);
        
        private void WriteCastShapeCandidates(Vector3 playerPos, Vector3 targetPos)
        {
            float dx = targetPos.X - playerPos.X;
            float dz = targetPos.Z - playerPos.Z;
            float len = (float)Math.Sqrt(dx * dx + dz * dz);
            if (len < 1e-4f) len = 1f;
            float tx = dx / len;
            float tz = dz / len;

            Span<Vector4> buf = stackalloc Vector4[CastShapeCandidateCount];
            int i = 0;

            buf[i++] = new Vector4(playerPos.X + tx * 2f, playerPos.Y, playerPos.Z + tz * 2f, 1f);

            foreach (var r in CastShapeRadii)
                foreach (var d in CastShapeDirs)
                    buf[i++] = new Vector4(
                        playerPos.X + d.dx * r,
                        playerPos.Y,
                        playerPos.Z + d.dz * r,
                        1f);

            memoryService.WriteBytes(
                Base + (int)MoveTarget.Candidates,
                MemoryMarshal.AsBytes(buf).ToArray());
        }
        
        private void InstallMoveTargetHook(Vector3 playerPos)
        {
            var physWorld =
                memoryService.Read<nint>(memoryService.Read<nint>(FrpgHavokManImp.Base) + FrpgHavokManImp.FrpgPhysWorld);
            var playerRagdoll = memoryService.FollowPointers(memoryService.Read<nint>(WorldChrManImp.Base),
            [
                WorldChrManImp.PlayerIns,
                ChrIns.ChrCtrl,
                ChrIns.ChrCtrlOffsets.RagdollIns
            ], true);

            var targetPhys = memoryService.FollowPointers(GetChrIns(),
                ChrIns.ChrPhysicsModule, true);
            var targetProxy = memoryService.Read<nint>(targetPhys + ChrIns.ChrPhysicsOffsets.CSChrProxy);
            var targetShape = memoryService.Read<nint>(targetProxy + ChrIns.CSChrProxyOffsets.Shape);
            var hkChrProxy = memoryService.Read<nint>(targetProxy + ChrIns.CSChrProxyOffsets.CsHkCharacterProxy);
            if (physWorld == 0 || playerRagdoll == 0 || targetPhys == 0 || targetProxy == 0 || targetShape == 0 || hkChrProxy == 0)
            {
                return;
            }

            var status = Base + (int)MoveTarget.Status;
            var target = Base + LockedTargetPtr;
            var candidates = Base + (int)MoveTarget.Candidates;
            var start = Base + (int)MoveTarget.Start;
            var delta = Base + (int)MoveTarget.Delta;
            var hitId = Base + (int)MoveTarget.HitId;
            var sweepRadius = Base + (int)MoveTarget.SweepRadius;
            var probeYOffset = Base + (int)MoveTarget.ProbeYOffset;
            var hitExtra = Base + (int)MoveTarget.HitExtra;
            var hitPos = Base + (int)MoveTarget.HitPos;
            var hitNormal = Base + (int)MoveTarget.HitNormal;
            var distRaw = Base + (int)MoveTarget.DistRaw;
            var code = Base + (int)MoveTarget.Code;
            
            
            memoryService.Write(sweepRadius,  CastShapeSweepRadius);
            memoryService.Write(probeYOffset, CastShapeProbeYOffset);
            memoryService.Write(status, 0);
            
            var startPos = new Vector4(playerPos.X, playerPos.Y + CastShapeProbeYOffset, playerPos.Z, 1f);
            memoryService.Write(start, startPos);

            var bytes = AsmLoader.GetAsmBytes(AsmScript.MoveTarget);
            AsmHelper.WriteRelativeOffsets(bytes, [
                (code, status, 7, 2),
                (code + 0xD, target, 7, 0xD + 3),
                (code + 0x59, candidates, 7, 0x59 + 3),
                (code + 0x81, start, 7, 0x81 + 3),
                (code + 0x8E, delta, 7, 0x8E + 3),
                (code + 0x95, hitId, 10, 0x95 + 2),
                (code + 0xA9, hitId, 7, 0xA9 + 3),
                (code + 0xB6, start, 7, 0xB6 + 3),
                (code + 0xBD, delta, 7, 0xBD + 3),
                (code + 0xC9, sweepRadius, 8, 0xC9 + 4),
                (code + 0xE6, hitExtra, 7, 0xE6 + 3),
                (code + 0xF2, hitPos, 7, 0xF2 + 3),
                (code + 0xFE, hitNormal, 7, 0xFE + 3),
                (code + 0x10A, distRaw, 7, 0x10A + 3),
                (code + 0x116, Functions.CastShape, 5, 0x116 + 1),
                (code + 0x11B, hitId, 10, 0x11B + 2),
                (code + 0x170, probeYOffset, 8, 0x170 + 4),
                (code + 0x1AE, Functions.WorldGetClosestPoints, 5, 0x1AE + 1),
                (code + 0x1C5, Functions.SetPosition, 5, 0x1C5 + 1),
                (code + 0x1CA, status, 7, 0x1CA + 2),
                (code + 0x1DB, status, 7, 0x1DB + 2),
                (code + 0x229, Hooks.ChrIns_PrePhysics + 8, 5, 0x229 + 1),
            ]);

            AsmHelper.WriteAbsoluteAddresses(bytes, [
            (hkChrProxy, 0x60 + 2),
            (physWorld, 0x9F + 2),
            (playerRagdoll, 0xD7 + 2),
            (physWorld, 0x18D + 2),
            (targetShape, 0x19C + 2),
            (targetPhys, 0x1B7 + 2)
            ]);

            memoryService.WriteBytes(code, bytes);
            hookManager.InstallHook(code, Hooks.ChrIns_PrePhysics, [0x49, 0x8B, 0x5B, 0x20, 0x49, 0x8B, 0x7B, 0x28]);
        }

        #endregion
    }
}