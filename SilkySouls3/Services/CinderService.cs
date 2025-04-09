using System;
using System.Collections.Generic;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;

namespace SilkySouls3.Services
{
    public class CinderService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        private int _currentPhase;

        private readonly Dictionary<int, int> _phaseAnimations = new Dictionary<int, int>
        {
            { 0, 20000 }, //Sword
            { 1, 20001 }, //Lance
            { 2, 20002 }, //Curved
            { 3, 20004 }, //Staff
            { 4, 20010 } //Gwyn
        };


        public CinderService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void ForcePhaseTransition(int phaseIndex)
        {
            _currentPhase = phaseIndex;
            int phaseAnimation = _phaseAnimations[phaseIndex];
            ForceAnimation(phaseAnimation);
            ResetLuaNumbers();
        }

        private void ForceAnimation(int phaseAnimation)
        {
            var forceAnimationPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrEventModule,
                    Offsets.WorldChrMan.ForceAnimationOffset
                }, false
            );
            _memoryIo.WriteInt32(forceAnimationPtr, phaseAnimation);
        }

        private void ResetLuaNumbers()
        {
            var luaNumbersPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    Offsets.EnemyIns.ComManipulator,
                    Offsets.EnemyIns.AiIns,
                    (int)Offsets.EnemyIns.AiInsOffsets.LuaNumbers
                }, false);
            
            _memoryIo.WriteFloat(luaNumbersPtr + (int) Offsets.EnemyIns.LuaNumbers.Gwyn5HitComboNumberIndex * 4, 0);
            _memoryIo.WriteFloat(luaNumbersPtr + (int) Offsets.EnemyIns.LuaNumbers.GwynLightningRainNumberIndex * 4, 0);
            _memoryIo.WriteFloat(luaNumbersPtr + (int) Offsets.EnemyIns.LuaNumbers.PhaseTransitionCounterNumberIndex * 4, 0);
        }
        
        
        public void ToggleCinderPhaseLock(bool enable)
        {
            var cinderPhaseLockCode = CodeCaveOffsets.Base + CodeCaveOffsets.CinderPhaseLock;
            if (enable)
            {
                var addSubGoalHook = Offsets.Hooks.AddSubGoal;
                byte[] phaseLockBytes = AsmLoader.GetAsmBytes("CinderPhaseLock");
                byte[] jmpBytes =
                    AsmHelper.GetRelOffsetBytes(cinderPhaseLockCode.ToInt64() + 0x28, addSubGoalHook + 0x6, 5);
                Array.Copy(jmpBytes, 0, phaseLockBytes, 0x28 + 1, 4);
                _memoryIo.WriteBytes(cinderPhaseLockCode, phaseLockBytes);
                _hookManager.InstallHook(cinderPhaseLockCode.ToInt64(), addSubGoalHook,
                    new byte[] { 0xF3, 0x0F, 0x11, 0x44, 0x24, 0x20 });
            }
            else
            {
                _hookManager.UninstallHook(cinderPhaseLockCode.ToInt64());
            }
        }

        public void CastSoulMass()
        {
            
        }
    }
}