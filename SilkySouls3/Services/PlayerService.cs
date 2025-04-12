using System;
using SilkySouls3.Memory;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class PlayerService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        public PlayerService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public int GetHp() =>
            _memoryIo.ReadInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Hp));

        public int GetMaxHp() =>
            _memoryIo.ReadInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.MaxHp));

        public void SetHp(int hp) =>
            _memoryIo.WriteInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Hp), hp);

        public void ToggleNoDamage(bool setValue)
        {
            var noDamagePtr = GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.ChrFlags2);
            var flagMask = (byte)WorldChrMan.ChrFlags2.NoDamage;
            _memoryIo.SetBitValue(noDamagePtr, flagMask, setValue);
        }


        private IntPtr GetChrDataFieldPtr(int fieldOffset)
        {
            return _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrDataModule,
                    fieldOffset
                }, false);
        }

        public void SavePos(int index)
        {
            var chrPhysicsModule = _memoryIo.FollowPointers(WorldChrMan.Base, new[]
            {
                WorldChrMan.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.Modules,
                (int)WorldChrMan.Modules.ChrPhysicsModule,
            }, true);

            byte[] positionBytes = _memoryIo.ReadBytes(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Coords, 12);
            float angle = _memoryIo.ReadFloat(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Angle);

            byte[] angleBytes = BitConverter.GetBytes(angle);
            byte[] data = new byte[16];
            Buffer.BlockCopy(positionBytes, 0, data, 0, 12);
            Buffer.BlockCopy(angleBytes, 0, data, 12, 4);

            if (index == 0) _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, data);
            else _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, data);
        }

        public void RestorePos(int index)
        {
            byte[] positionBytes;
            if (index == 0) positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 16);
            else positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 16);

            float angle = BitConverter.ToSingle(positionBytes, 12);

            var chrPhysicsModule = _memoryIo.FollowPointers(WorldChrMan.Base, new[]
            {
                WorldChrMan.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.Modules,
                (int)WorldChrMan.Modules.ChrPhysicsModule,
            }, true);

            byte[] xyzBytes = new byte[12];
            Buffer.BlockCopy(positionBytes, 0, xyzBytes, 0, 12);

            _memoryIo.WriteBytes(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Coords, xyzBytes);
            _memoryIo.WriteFloat(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Angle, angle);
        }

        public void ToggleDebugFlag(int offset, int value)
        {
            _memoryIo.WriteByte(DebugFlags.Base + offset, value);
        }

        public void ToggleInfiniteDurability(bool isInfiniteDurabilityEnabled)
        {
            _memoryIo.WriteByte(Patches.InfiniteDurability + 0x1, isInfiniteDurabilityEnabled ? 0x84 : 0x85);
        }

        public void ToggleInfinitePoise(bool setValue)
        {
            var infinitePoisePtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.InfinitePoise
                }, false);
            var flagMask = WorldChrMan.InfinitePoise;
            _memoryIo.SetBitValue(infinitePoisePtr, flagMask, setValue);
        }

        public void ToggleNoGoodsConsume(bool setValue)
        {
            var noGoodsConsumePtr = _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.CharFlags1
                }, false);
            var flagMask = (int)WorldChrMan.ChrFlag1BitFlag.NoGoodsConsume;
            _memoryIo.SetBit32(noGoodsConsumePtr, flagMask, setValue);
        }

        public int GetNewGame() =>
            _memoryIo.ReadInt32((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + GameDataMan.NewGame);

        public void SetNewGame(int value) =>
            _memoryIo.WriteInt32((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + GameDataMan.NewGame, value);
    }
}