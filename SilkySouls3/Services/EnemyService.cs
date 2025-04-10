using System;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;

namespace SilkySouls3.Services
{
    public class EnemyService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        private IntPtr _saveTargetPtrCode;

        public EnemyService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }
        
        public void InstallTargetHook()
        {
            var saveTargetHook = Offsets.Hooks.LastLockedTarget;
            var savedTargetPtr = CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr;
            _saveTargetPtrCode = CodeCaveOffsets.Base + CodeCaveOffsets.SaveTargetPtrCode;

            byte[] saveTargetPtrBytes = AsmLoader.GetAsmBytes("SaveTargetPtr");
            byte[] bytes = AsmHelper.GetRelOffsetBytes(_saveTargetPtrCode.ToInt64(), savedTargetPtr.ToInt64(), 7);
            Array.Copy(bytes, 0, saveTargetPtrBytes, 0x3, 4);
            bytes = AsmHelper.GetRelOffsetBytes(_saveTargetPtrCode.ToInt64() + 0xE, saveTargetHook + 0x7, 5);
            Array.Copy(bytes, 0, saveTargetPtrBytes, 0xE + 1, 4);

            _memoryIo.WriteBytes(_saveTargetPtrCode, saveTargetPtrBytes);
            _hookManager.InstallHook(_saveTargetPtrCode.ToInt64(), saveTargetHook,
                new byte[] { 0x48, 0x8B, 0x80, 0x90, 0x1F, 0x00, 0x00 });
        }

        public void UninstallTargetHook()
        {
            _hookManager.UninstallHook(_saveTargetPtrCode.ToInt64());
        }

        public ulong GetTargetId()
        {
            return _memoryIo.ReadUInt64(CodeCaveOffsets.Base +
                                        CodeCaveOffsets.LockedTargetPtr);
        }
        
        public int GetTargetHp() =>
            _memoryIo.ReadInt32(GetTargetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.Hp));

        public int GetTargetMaxHp() =>
            _memoryIo.ReadInt32(GetTargetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.MaxHp));

        public void SetTargetHp(int health) =>
            _memoryIo.WriteInt32(GetTargetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.Hp), health);

        public void ToggleTargetNoDamage(bool isFreezeHealthEnabled)
        {
            var targetChrDataPtr = GetTargetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.ChrFlags2);
            _memoryIo.SetBitValue(targetChrDataPtr, (byte)Offsets.WorldChrMan.ChrFlags2.NoDamage, isFreezeHealthEnabled);
        }

        public bool IsTargetNoDamageEnabled() =>
            _memoryIo.IsBitSet(GetTargetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.ChrFlags2),
                (byte)Offsets.WorldChrMan.ChrFlags2.NoDamage);

        private IntPtr GetTargetChrDataFieldPtr(int fieldOffset)
        {
            return _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrDataModule,
                    fieldOffset
                }, false);
        }

        public (bool Poison, bool Toxic, bool Bleed, bool Frost) GetImmunities()
        {
            var spEffectBasePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    Offsets.EnemyIns.ComManipulator,
                    (int)Offsets.EnemyIns.ComManipOffsets.AiIns,
                    (int)Offsets.EnemyIns.AiInsOffsets.SpEffectPtr
                }, true);

            return (
                _memoryIo.ReadInt32(spEffectBasePtr + (int)Offsets.EnemyIns.SpEffectImmunityOffsets.Poison) == 30000,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)Offsets.EnemyIns.SpEffectImmunityOffsets.Toxic) == 30010,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)Offsets.EnemyIns.SpEffectImmunityOffsets.Bleed) == 30020,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)Offsets.EnemyIns.SpEffectImmunityOffsets.FrostBite) == 30040
                );
        }

        public int GetTargetResistance(Offsets.WorldChrMan.ChrResistModule resistanceOffset)
        {
            var resistancePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrResistModule,
                    (int) resistanceOffset
                }, false);

            return _memoryIo.ReadInt32(resistancePtr);
        }

        public float GetTargetPoise(Offsets.WorldChrMan.ChrSuperArmorModule poiseOffset)
        {
            var poisePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrSuperArmorModule,
                    (int) poiseOffset
                }, false);
            
            return _memoryIo.ReadFloat(poisePtr);
        }

        public void ToggleTargetAi(bool isDisableTargetAiEnabled)
        {
            var targetPtr = (IntPtr) _memoryIo.ReadInt64(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
            
            _memoryIo.SetBit32(targetPtr + (int) Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags1,
                (int)Offsets.WorldChrMan.ChrFlag1BitFlag.DisableAi, isDisableTargetAiEnabled);
        }

        public bool IsTargetAiDisabled()
        {
            var targetPtr = (IntPtr) _memoryIo.ReadInt64(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.IsBitSet(targetPtr + (int)Offsets.WorldChrMan.PlayerInsOffsets.ChrFlags1,
                (int)Offsets.WorldChrMan.ChrFlag1BitFlag.DisableAi);
        }

        public void SetTargetSpeed(float value)
        {
            var speedPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrBehaviorModule,
                    (int) Offsets.WorldChrMan.ChrBehaviorModule.AnimSpeed
                }, false);
            
            _memoryIo.WriteFloat(speedPtr, value);
        }

        public float GetTargetSpeed()
        {
            var speedPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrBehaviorModule,
                    (int) Offsets.WorldChrMan.ChrBehaviorModule.AnimSpeed
                }, false);
            
            return _memoryIo.ReadFloat(speedPtr);
        }

        public float[] GetTargetPos()
        {
            var targetPosPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrPhysicsModule,
                    Offsets.WorldChrMan.CsChrProxy,
                    Offsets.WorldChrMan.CsHkCharacterProxy,
                    Offsets.WorldChrMan.TargetCoordsOffset
                }, false);
            
            float[] position = new float[3];
            position[0] = _memoryIo.ReadFloat(targetPosPtr);
            position[1] = _memoryIo.ReadFloat(targetPosPtr + 0x4);
            position[2] = _memoryIo.ReadFloat(targetPosPtr + 0x8);

            return position;
        }

        public void InstallRepeatActHook()
        {
            var savedBattleGoalIdLoc = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.BattleGoalId;
            var repeatActCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.RepeatAct.Code;
            var hookLoc = Offsets.Hooks.RepeatAct;
            
            var enemyBattleGoalIdPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    Offsets.EnemyIns.ComManipulator,
                    (int)Offsets.EnemyIns.ComManipOffsets.AiIns,
                    (int)Offsets.EnemyIns.AiInsOffsets.NpcThinkParam,
                    (int)Offsets.EnemyIns.NpcThinkParam.BattleGoalId
                }, false);
            
            _memoryIo.WriteInt32(savedBattleGoalIdLoc, _memoryIo.ReadInt32(enemyBattleGoalIdPtr));

            byte[] repeatActBytes = AsmLoader.GetAsmBytes("TargetRepeatAct");
            byte[] bytes = AsmHelper.GetRelOffsetBytes(repeatActCode + 0x1, savedBattleGoalIdLoc, 0x6);
            Array.Copy(bytes, 0, repeatActBytes, 0x1 + 2, 4);
            bytes = AsmHelper.GetJmpOriginOffsetBytes(hookLoc, 7, repeatActCode + 0x19);
            Array.Copy(bytes, 0, repeatActBytes, 0x14 + 1, 4);
            bytes = AsmHelper.GetJmpOriginOffsetBytes(hookLoc, 7, repeatActCode + 0x26);
            Array.Copy(bytes, 0, repeatActBytes, 0x21 + 1, 4);
            _memoryIo.WriteBytes(repeatActCode, repeatActBytes);

            _hookManager.InstallHook(repeatActCode.ToInt64(), hookLoc,
                new byte[] { 0x0F, 0xBE, 0x80, 0x81, 0xB6, 0x00, 0x00  });
        }
    }
}