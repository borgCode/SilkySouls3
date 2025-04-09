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
                    Offsets.EnemyIns.AiIns,
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
    }
}