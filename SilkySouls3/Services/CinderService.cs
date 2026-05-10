using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class CinderService : ICinderService
    {
        private readonly IMemoryService _memoryService;
        private readonly HookManager _hookManager;
        private readonly IChrInsService _chrInsService;
        private readonly ISpEffectService _spEffectService;
        private readonly IParamService _paramService;
        private readonly IReminderService _reminderService;

        private const int CinderEntityId = 4100800;
        
        private const int CinderNpcParamId = 528000;
        private const int StaggerParamOffset = 0x50;
        
        private const int SoulmassSpEffectId = 12120;
        private const int RemoveSoulmassSpEffectId = 12121;
        private const int SpEffectDurationOffset = 0x8;
        private nint _cinderChrIns;

        private const int Gwyn5HitComboNumberIndex = 0;
        private const int GwynLightningRainNumberIndex = 1;
        private const int PhaseTransitionCounterNumberIndex = 2;

        private static readonly Dictionary<CinderPhase, int> PhaseAnimations = new()
        {
            { CinderPhase.Sword, 20000 },
            { CinderPhase.Lance, 20001 },
            { CinderPhase.Curved, 20002 },
            { CinderPhase.Staff, 20004 },
            { CinderPhase.Gwyn, 20010 },
        };

        public CinderService(IMemoryService memoryService, HookManager hookManager, IChrInsService chrInsService,
            IStateService stateService, ISpEffectService spEffectService, IParamService paramService,
            IReminderService reminderService)
        {
            _memoryService = memoryService;
            _hookManager = hookManager;
            _chrInsService = chrInsService;
            _spEffectService = spEffectService;
            _paramService = paramService;
            _reminderService = reminderService;
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
        }

        #region Public

        public void ForcePhaseTransition(CinderPhase phase)
        {
            if (!IsValidCinderFight()) return;
            _chrInsService.RequestEventAnimation(_cinderChrIns, PhaseAnimations[phase]);
            ResetLuaNumbers();
        }

        public void ToggleCinderPhaseLock(bool enable)
        {
            var cinderPhaseLockCode = CustomCodeOffsets.Base + CustomCodeOffsets.CinderPhaseLock;
            if (enable)
            {
                _reminderService.TrySetReminder();
                
                byte[] phaseLockBytes = AsmLoader.GetAsmBytes(AsmScript.CinderPhaseLock);
                byte[] jmpBytes =
                    AsmHelper.GetRelOffsetBytes(cinderPhaseLockCode + 0x2C, Hooks.AddSubGoal + 10, 5);
                Array.Copy(jmpBytes, 0, phaseLockBytes, 0x2C + 1, 4);
                _memoryService.WriteBytes(cinderPhaseLockCode, phaseLockBytes);
                _hookManager.InstallHook(cinderPhaseLockCode, Hooks.AddSubGoal,
                    [0x48, 0x8B, 0xC4, 0x48, 0x81, 0xEC, 0x98, 0x00, 0x00, 0x00]);
            }
            else
            {
                _hookManager.UninstallHook(cinderPhaseLockCode);
            }
        }

        public void CastSoulMass()
        {
            if (!IsValidCinderFight()) return;

            Task.Run(() =>
            {
                const int soulMassAnimationId = 3003;
                const int timeActCurrentAnimationId = 4003003;
                const int maxWaitTime = 500;

                int phaseBeforeSoulmass = ReadCinderPhaseMask();

                if (phaseBeforeSoulmass != (int)CinderPhase.Staff)
                {
                    ForcePhaseTransition(CinderPhase.Staff);

                    if (!WaitForGameState(
                            () => ReadCinderPhaseMask() == (int)CinderPhase.Staff,
                            maxWaitTime,
                            "Something went wrong with the forced transition, please try again or reload the game"))
                    {
                        return;
                    }
                }

                _chrInsService.RequestEventAnimation(_cinderChrIns, soulMassAnimationId);

                var chrTimeAct = _memoryService.FollowPointers(_cinderChrIns, ChrIns.ChrTimeActModule, true);

                if (!WaitForGameState(
                        () => _memoryService.Read<int>(chrTimeAct + ChrIns.ChrTimeActOffsets.CurrentAnimationId) ==
                              timeActCurrentAnimationId, maxWaitTime,
                        "Something went wrong with the forced soulmass, please try again or reload the game"))
                {
                    return;
                }

                if (!WaitForGameState(
                        () => _memoryService.Read<int>(chrTimeAct + ChrIns.ChrTimeActOffsets.CurrentAnimationId) !=
                              timeActCurrentAnimationId, maxWaitTime,
                        "Soulmass animation didn't finish in time"))
                {
                    return;
                }

                if (phaseBeforeSoulmass != (int)CinderPhase.Staff
                    && PhaseAnimations.ContainsKey((CinderPhase)phaseBeforeSoulmass))
                {
                    ForcePhaseTransition((CinderPhase)phaseBeforeSoulmass);
                }
            });
        }

        
        public void RemoveSoulmass()
        {
            if (!IsValidCinderFight()) return;
            
            _spEffectService.ApplySpEffect(_cinderChrIns, RemoveSoulmassSpEffectId);
        }

        public void ToggleEndlessSoulmass(bool isEnabled)
        {
            
            var soulmassRow = _paramService.GetParamRow((int)Param.SpEffectParam, SoulmassSpEffectId);
            _paramService.Write(soulmassRow, SpEffectDurationOffset, isEnabled ? -1.0f : 40.0f);

            if (!isEnabled) return;
            _reminderService.TrySetReminder();
            if (_spEffectService.HasSpEffect(_cinderChrIns, SoulmassSpEffectId))
            {
                _spEffectService.ApplySpEffect(_cinderChrIns, SoulmassSpEffectId);
            }
            else
            {
                CastSoulMass();
            }

        }

        public void ToggleNoSoulmassRemoveOnStagger(bool isEnabled)
        {
            var preventSoulmassStaggerRemovalCode = CustomCodeOffsets.Base + CustomCodeOffsets.CinderSoulmassRemoval;

            if (isEnabled)
            {
                _reminderService.TrySetReminder();
                var hookLoc = Hooks.SoulmassStaggerRemoval;
                int endOfFuncOffset = 0x5A;
                var codeBytes = AsmLoader.GetAsmBytes(AsmScript.CinderSoulmassRemoval);
                AsmHelper.WriteRelativeOffsets(codeBytes, [
                    (preventSoulmassStaggerRemovalCode + 0xB, hookLoc + endOfFuncOffset, 6, 0xB + 2),
                    (preventSoulmassStaggerRemovalCode + 0x11, hookLoc + 0x5, 5, 0x11 + 1)
                ]);
            
                _memoryService.WriteBytes(preventSoulmassStaggerRemovalCode, codeBytes);
                _hookManager.InstallHook(preventSoulmassStaggerRemovalCode, hookLoc,
                    [0xC6, 0x44, 0x24, 0x40, 0x00]);
            }
            else
            {
                _hookManager.UninstallHook(preventSoulmassStaggerRemovalCode);
            }
            
        }

        public void ToggleCinderStagger(bool isEnabled)
        {
            if (isEnabled) _reminderService.TrySetReminder();

            var npcParam = _paramService.GetParamRow((int)Param.NpcParam, CinderNpcParamId);
            _paramService.Write(npcParam, StaggerParamOffset, isEnabled ? 0 : 5360);
        }

        #endregion

        #region Private

        private void OnGameNotLoaded()
        {
            _cinderChrIns = 0;
        }

        private bool IsValidCinderFight()
        {
            if (_cinderChrIns != 0) return true;
            _cinderChrIns = _chrInsService.GetChrInsByEntityId(CinderEntityId);
            return _cinderChrIns != 0;
        }
        
        private static readonly int ValidPhaseBits =
            ((CinderPhase[])Enum.GetValues(typeof(CinderPhase))).Aggregate(0, (acc, p) => acc | (int)p);

        private int ReadCinderPhaseMask()
        {
            bool appliedTaePrimMask = _memoryService.IsBitSet(_cinderChrIns + ChrIns.IsTaePrimMaskActive.Offset,
                ChrIns.IsTaePrimMaskActive.Bit);
            if (appliedTaePrimMask) return _memoryService.Read<int>(_cinderChrIns + ChrIns.ChrPrimMaskOffset);

            int seeded = _memoryService.Read<int>(_cinderChrIns + ChrIns.ChrPrimMaskMirrorOffset);
            if ((seeded & ValidPhaseBits) != 0) return seeded;

            nint model = _memoryService.Read<nint>(_cinderChrIns + ChrIns.ChrModel);
            return model != 0 ? _memoryService.Read<int>(model + ChrIns.ChrModelOffsets.ChrModelPrimMask) : 0;
        }

        private void ResetLuaNumbers()
        {
            var luaNumbers = _memoryService.FollowPointers(_cinderChrIns,
                [..ChrIns.AiThink, ChrIns.AiThinkOffsets.LuaNumbers], false);
            

            _memoryService.Write(luaNumbers + Gwyn5HitComboNumberIndex * 4, 0f);
            _memoryService.Write(luaNumbers + GwynLightningRainNumberIndex * 4, 0f);
            _memoryService.Write(luaNumbers + PhaseTransitionCounterNumberIndex * 4, 0f);
        }
        
        private bool WaitForGameState(Func<bool> condition, int maxWaitTime, string errorMessage)
        {
            int waitCounter = 0;

            while (!condition())
            {
                Thread.Sleep(10);
                waitCounter++;

                if (waitCounter > maxWaitTime)
                {
                    MsgBox.Show(errorMessage, "Operation Failed");
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}