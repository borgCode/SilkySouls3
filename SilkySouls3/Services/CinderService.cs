using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class CinderService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private readonly Dictionary<int, int> _phaseAnimations = new Dictionary<int, int>
        {
            { 0, 20000 }, //Sword
            { 1, 20001 }, //Lance
            { 2, 20002 }, //Curved
            { 3, 20004 }, //Staff
            { 4, 20010 } //Gwyn
        };

        private readonly Dictionary<int, int> _currentPhaseLookUp = new Dictionary<int, int>
        {
            { 1 << 1, 0 }, //Sword
            { 1 << 4, 1 }, //Lance
            { 1 << 2, 2 }, //Curved
            { 1 << 3, 3 }, //Staff
            { 1 << 5, 4 }, //Gwyn
        };


        public CinderService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void ForcePhaseTransition(int phaseIndex)
        {
            if (!IsTargetCinder()) return;
            int phaseAnimation = _phaseAnimations[phaseIndex];
            ForceAnimation(phaseAnimation);
            ResetLuaNumbers();
        }

        private void ForceAnimation(int phaseAnimation)
        {
            var forceAnimationPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrEventModule,
                    WorldChrMan.ForceAnimationOffset
                }, false
            );
            _memoryIo.WriteInt32(forceAnimationPtr, phaseAnimation);
        }

        private void ResetLuaNumbers()
        {
            var luaNumbersPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.AiIns,
                    (int)EnemyIns.AiInsOffsets.LuaNumbers
                }, false);

            _memoryIo.WriteFloat(luaNumbersPtr + (int)EnemyIns.LuaNumbers.Gwyn5HitComboNumberIndex * 4, 0);
            _memoryIo.WriteFloat(luaNumbersPtr + (int)EnemyIns.LuaNumbers.GwynLightningRainNumberIndex * 4, 0);
            _memoryIo.WriteFloat(luaNumbersPtr + (int)EnemyIns.LuaNumbers.PhaseTransitionCounterNumberIndex * 4,
                0);
        }


        public void ToggleCinderPhaseLock(bool enable)
        {
            if (!IsTargetCinder()) return;
            var cinderPhaseLockCode = CodeCaveOffsets.Base + CodeCaveOffsets.CinderPhaseLock;
            if (enable)
            {
                var addSubGoalHook = Hooks.AddSubGoal;
                byte[] phaseLockBytes = AsmLoader.GetAsmBytes("CinderPhaseLock");
                byte[] jmpBytes =
                    AsmHelper.GetRelOffsetBytes(cinderPhaseLockCode.ToInt64() + 0x2C, addSubGoalHook + 10, 5);
                Array.Copy(jmpBytes, 0, phaseLockBytes, 0x2C + 1, 4);
                _memoryIo.WriteBytes(cinderPhaseLockCode, phaseLockBytes);
                _hookManager.InstallHook(cinderPhaseLockCode.ToInt64(), addSubGoalHook,
                    new byte[] { 0x48, 0x8B, 0xC4, 0x48, 0x81, 0xEC, 0x98, 0x00, 0x00, 0x00 });
            }
            else
            {
                _hookManager.UninstallHook(cinderPhaseLockCode.ToInt64());
            }
        }

        public void CastSoulMass()
        {
            if (!IsTargetCinder()) return;

            Task.Run(() =>
            {
                const int soulmassEzStateId = 3003;
                const string soulmassAnimationName = "Attack3003";
                const int stringLength = 20;
                const int maxWaitTime = 500;

                var targetPtr = (IntPtr)_memoryIo.ReadInt64(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
                var currentPhaseAddr = targetPtr + EnemyIns.CurrentPhaseOffset;
                int phaseBeforeSoulmass = _memoryIo.ReadInt32(currentPhaseAddr);
                if (phaseBeforeSoulmass == 0) phaseBeforeSoulmass = 1 << 1;

                ForceAnimation(_phaseAnimations[3]);

                if (!WaitForGameState(
                        () => _memoryIo.ReadInt32(currentPhaseAddr) == 1 << 3,
                        maxWaitTime,
                        "Something went wrong with the forced transition, please try again or reload the game"))
                {
                    return;
                }

                var modulesPtr =
                    (IntPtr)_memoryIo.ReadInt64(targetPtr + (int)WorldChrMan.PlayerInsOffsets.Modules);
                var chrEventModulePtr =
                    (IntPtr)_memoryIo.ReadInt64(modulesPtr + (int)WorldChrMan.Modules.ChrEventModule);
                _memoryIo.WriteInt32(chrEventModulePtr + WorldChrMan.ForceAnimationOffset, soulmassEzStateId);

                var chrBehaviorModulePtr =
                    (IntPtr)_memoryIo.ReadInt64(modulesPtr + (int)WorldChrMan.Modules.ChrBehaviorModule);

                if (!WaitForGameState(
                        () => _memoryIo.ReadString(
                            chrBehaviorModulePtr + (int)WorldChrMan.ChrBehaviorModule.CurrentAnimation,
                            stringLength) == soulmassAnimationName, maxWaitTime,
                        "Something went wrong with the forced soulmass, please try again or reload the game"))
                {
                    return;
                }

                if (!WaitForGameState(
                        () => _memoryIo.ReadString(
                            chrBehaviorModulePtr + (int)WorldChrMan.ChrBehaviorModule.CurrentAnimation,
                            stringLength) != soulmassAnimationName, maxWaitTime,
                        "Soulmass animation didn't start in time"))
                {
                    return;
                }

                int previousPhase = _currentPhaseLookUp[phaseBeforeSoulmass];
                ForceAnimation(_phaseAnimations[previousPhase]);
            });
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
                    MessageBox.Show(errorMessage, "Operation Failed");
                    return false;
                }
            }

            return true;
        }

        public void ToggleEndlessSoulmass(bool isEnabled)
        {
            if (!IsTargetCinder()) return;
            var soulmassPtr = _memoryIo.FollowPointers(SoloParamRepo.Base,
                new[]
                {
                    SoloParamRepo.SpEffectParamResCap,
                    SoloParamRepo.SpEffectPtr1,
                    SoloParamRepo.SpEffectPtr2,
                    SoloParamRepo.CinderSoulmass
                }, false);
            if (isEnabled)
            {
                _memoryIo.WriteFloat(soulmassPtr + 0x8, -1.0f);
                CastSoulMass();
            }
            else _memoryIo.WriteFloat(soulmassPtr + 0x8, 40.0f);
        }

        public bool IsTargetCinder()
        {
            var enemyIdPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.EnemyId
                }, false);
            return _memoryIo.ReadInt32(enemyIdPtr) == 528000;
        }

        public void ToggleCinderStagger(bool isCinderNoStaggerEnabled)
        {
            var spEffectBasePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.AiIns,
                    (int)EnemyIns.AiInsOffsets.SpEffectPtr
                }, true);

            _memoryIo.WriteInt32(spEffectBasePtr + (int)EnemyIns.SpEffectOffsets.Stagger,
                isCinderNoStaggerEnabled ? 0 : 5360);
        }
    }
}