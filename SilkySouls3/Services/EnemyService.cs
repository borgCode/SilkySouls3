using System;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

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
            var saveTargetHook = Hooks.LastLockedTarget;
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
            _memoryIo.ReadInt32(GetTargetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Hp));

        public int GetTargetMaxHp() =>
            _memoryIo.ReadInt32(GetTargetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.MaxHp));

        public void SetTargetHp(int health) =>
            _memoryIo.WriteInt32(GetTargetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Hp), health);

        public void ToggleTargetNoDamage(bool isFreezeHealthEnabled)
        {
            var targetChrDataPtr = GetTargetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.ChrFlags2);
            _memoryIo.SetBitValue(targetChrDataPtr, (byte)WorldChrMan.ChrFlags2.NoDamage, isFreezeHealthEnabled);
        }

        public bool IsTargetNoDamageEnabled() =>
            _memoryIo.IsBitSet(GetTargetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.ChrFlags2),
                (byte)WorldChrMan.ChrFlags2.NoDamage);

        private IntPtr GetTargetChrDataFieldPtr(int fieldOffset)
        {
            return _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrDataModule,
                    fieldOffset
                }, false);
        }

        public (bool Poison, bool Toxic, bool Bleed, bool Frost) GetImmunities()
        {
            var spEffectBasePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.AiIns,
                    (int)EnemyIns.AiInsOffsets.SpEffectPtr
                }, true);

            return (
                _memoryIo.ReadInt32(spEffectBasePtr + (int)EnemyIns.SpEffectOffsets.Poison) == 30000,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)EnemyIns.SpEffectOffsets.Toxic) == 30010,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)EnemyIns.SpEffectOffsets.Bleed) == 30020,
                _memoryIo.ReadInt32(spEffectBasePtr + (int)EnemyIns.SpEffectOffsets.FrostBite) == 30040
            );
        }

        public int GetTargetResistance(WorldChrMan.ChrResistModule resistanceOffset)
        {
            var resistancePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrResistModule,
                    (int)resistanceOffset
                }, false);

            return _memoryIo.ReadInt32(resistancePtr);
        }

        public float GetTargetPoise(WorldChrMan.ChrSuperArmorModule poiseOffset)
        {
            var poisePtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrSuperArmorModule,
                    (int)poiseOffset
                }, false);

            return _memoryIo.ReadFloat(poisePtr);
        }

        public void ToggleTargetAi(bool isDisableTargetAiEnabled)
        {
            var targetPtr = (IntPtr)_memoryIo.ReadInt64(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
            _memoryIo.SetBit32(targetPtr + (int)WorldChrMan.PlayerInsOffsets.CharFlags1,
                (int)WorldChrMan.ChrFlag1BitFlag.DisableAi, isDisableTargetAiEnabled);
        }

        public bool IsTargetAiDisabled()
        {
            var targetPtr = (IntPtr)_memoryIo.ReadInt64(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr);
            return _memoryIo.IsBitSet(targetPtr + (int)WorldChrMan.PlayerInsOffsets.CharFlags1,
                (int)WorldChrMan.ChrFlag1BitFlag.DisableAi);
        }

        public void SetTargetSpeed(float value)
        {
            var speedPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrBehaviorModule,
                    (int)WorldChrMan.ChrBehaviorModule.AnimSpeed
                }, false);

            _memoryIo.WriteFloat(speedPtr, value);
        }

        public float GetTargetSpeed()
        {
            var speedPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrBehaviorModule,
                    (int)WorldChrMan.ChrBehaviorModule.AnimSpeed
                }, false);

            return _memoryIo.ReadFloat(speedPtr);
        }

        public float[] GetTargetPos()
        {
            var targetPosPtr = _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrPhysicsModule,
                    WorldChrMan.CsChrProxy,
                    WorldChrMan.CsHkCharacterProxy,
                    WorldChrMan.TargetCoordsOffset
                }, false);

            float[] position = new float[3];
            position[0] = _memoryIo.ReadFloat(targetPosPtr);
            position[1] = _memoryIo.ReadFloat(targetPosPtr + 0x4);
            position[2] = _memoryIo.ReadFloat(targetPosPtr + 0x8);

            return position;
        }

        public void ToggleTargetRepeatAct(bool isEnabled)
        {
            var ptr = GetForceActPtr();
            _memoryIo.WriteUInt8(ptr, isEnabled ? _memoryIo.ReadUInt8(ptr + 1) : (byte)0);
        }

        public bool IsTargetRepeating() => _memoryIo.ReadUInt8(GetForceActPtr()) != 0;

        private IntPtr GetForceActPtr()
        {
            return _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.AiIns,
                    (int)EnemyIns.AiInsOffsets.AiFunc,
                    EnemyIns.ForceActPtr,
                    EnemyIns.ForceActOffset
                }, false);
        }

        public void ToggleDebugFlag(int offset, int value)
        {
            _memoryIo.WriteByte(DebugFlags.Base + offset, value);
        }

        public void ToggleAllRepeatAct(bool isAllRepeatActEnabled)
        {
            _memoryIo.WriteByte(Patches.RepeatAct, isAllRepeatActEnabled ? 0x82 : 0x81);
        }

        public void ForceAct(int forceAct) => _memoryIo.WriteUInt8(GetForceActPtr(), (byte)forceAct);
        public int GetLastAct() => _memoryIo.ReadUInt8(GetForceActPtr() + 1);
        public int GetForceAct() => _memoryIo.ReadUInt8(GetForceActPtr());

        public void ToggleTargetingView(bool isTargetingViewEnabled) =>
            _memoryIo.WriteByte(GetTargetingViewPtr(), isTargetingViewEnabled ? 6 : 0);

        public bool IsTargetViewEnabled() => _memoryIo.ReadUInt8(GetTargetingViewPtr()) != 0;

        public IntPtr GetTargetingViewPtr() =>
            _memoryIo.FollowPointers(CodeCaveOffsets.Base + CodeCaveOffsets.LockedTargetPtr,
                new[]
                {
                    EnemyIns.ComManipulator,
                    (int)EnemyIns.ComManipOffsets.AiIns,
                    (int)EnemyIns.AiInsOffsets.TargetingSystem,
                    EnemyIns.TargetingView
                }, false);
    }
}