using System;
using System.Collections.Generic;
using SilkySouls3.Memory;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class PlayerService
    {
        private readonly MemoryIo _memoryIo;
        
        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new Dictionary<int, int>
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };

        public PlayerService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
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
            var chrPhysicsModule = GetChrPhysicsModule();

            byte[] positionBytes = _memoryIo.ReadBytes(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.X, 12);
            float angle = _memoryIo.ReadFloat(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Angle);

            byte[] angleBytes = BitConverter.GetBytes(angle);
            byte[] data = new byte[16];
            Buffer.BlockCopy(positionBytes, 0, data, 0, 12);
            Buffer.BlockCopy(angleBytes, 0, data, 12, 4);

            if (index == 0) _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, data);
            else _memoryIo.WriteBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, data);
        }

        private IntPtr GetChrPhysicsModule()
        {
            return _memoryIo.FollowPointers(WorldChrMan.Base, new[]
            {
                WorldChrMan.PlayerIns,
                (int)WorldChrMan.PlayerInsOffsets.Modules,
                (int)WorldChrMan.Modules.ChrPhysicsModule,
            }, true);
        }

        public void RestorePos(int index)
        {
            byte[] positionBytes;
            if (index == 0) positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos1, 16);
            else positionBytes = _memoryIo.ReadBytes(CodeCaveOffsets.Base + CodeCaveOffsets.SavePos2, 16);

            float angle = BitConverter.ToSingle(positionBytes, 12);

            var chrPhysicsModule = GetChrPhysicsModule();

            byte[] xyzBytes = new byte[12];
            Buffer.BlockCopy(positionBytes, 0, xyzBytes, 0, 12);

            _memoryIo.WriteBytes(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.X, xyzBytes);
            _memoryIo.WriteFloat(chrPhysicsModule + (int)WorldChrMan.ChrPhysicsModule.Angle, angle);
        }

        public void SetAxis(WorldChrMan.ChrPhysicsModule axis, float value) =>
            _memoryIo.WriteFloat(GetChrPhysicsModule() + (int) axis, value);

        public (float x, float y, float z) GetCoords()
        {
            var coordBytes = _memoryIo.ReadBytes(GetChrPhysicsModule() + (int)WorldChrMan.ChrPhysicsModule.X, 12);
            float x = BitConverter.ToSingle(coordBytes, 0);
            float z = BitConverter.ToSingle(coordBytes, 4);
            float y = BitConverter.ToSingle(coordBytes, 8);
            return (x, y, z); 
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
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int) WorldChrMan.Modules.ChrSuperArmorModule,
                    (int) WorldChrMan.ChrSuperArmorModule.InfinitePoise,
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

        public int GetPlayerStat(GameDataMan.Stats stat)
        {
            var statsBasePtr = (IntPtr) _memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + GameDataMan.PlayerGameData);
            return _memoryIo.ReadInt32(statsBasePtr + (int)stat);
        }

        public void SetPlayerStat(GameDataMan.Stats stat, int newValue)
        {
            var statsBasePtr = (IntPtr) _memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + GameDataMan.PlayerGameData);
            int currentVal = _memoryIo.ReadInt32(statsBasePtr + (int)stat);
            if (currentVal == newValue) return;
            
            if (stat == GameDataMan.Stats.Souls) HandleSoulEdit(statsBasePtr, newValue, currentVal);
            else HandleStatEdit(statsBasePtr, stat, newValue, currentVal);
        }

        private void HandleSoulEdit(IntPtr statsBasePtr, int newValue, int currentVal)
        {
            if (newValue < currentVal)
            {
                _memoryIo.WriteInt32(statsBasePtr + (int) GameDataMan.Stats.Souls, newValue);
                return;
            }

            int difference = newValue - currentVal;
            int currentTotalSouls = _memoryIo.ReadInt32(statsBasePtr + (int)GameDataMan.Stats.TotalSouls);
            _memoryIo.WriteInt32(statsBasePtr + (int)GameDataMan.Stats.TotalSouls, difference + currentTotalSouls);
            _memoryIo.WriteInt32(statsBasePtr + (int) GameDataMan.Stats.Souls, newValue);
        }

        private void HandleStatEdit(IntPtr statsBasePtr, GameDataMan.Stats stat, int newValue, int currentVal)
        {
            var validatedStat = newValue;
            if (validatedStat < 1) validatedStat = 1;
            if (validatedStat > 99) validatedStat = 99;
            if (validatedStat == currentVal) return;
            _memoryIo.WriteInt32(statsBasePtr + (int) stat, newValue);
            
            int currentSoulLevel = _memoryIo.ReadInt32(statsBasePtr + (int)GameDataMan.Stats.SoulLevel);
            int newLevel = currentSoulLevel + (validatedStat - currentVal);
            
            _memoryIo.WriteInt32(statsBasePtr + (int)GameDataMan.Stats.SoulLevel, newLevel);
            if (newLevel < currentSoulLevel) return;
            
            int totalSoulsRequired = CalculateTotalSoulsRequired(currentSoulLevel, newLevel);
            int currentTotalSouls = _memoryIo.ReadInt32(statsBasePtr + (int)GameDataMan.Stats.TotalSouls);
            _memoryIo.WriteInt32(statsBasePtr + (int)GameDataMan.Stats.TotalSouls, totalSoulsRequired + currentTotalSouls);
        }

        public void GiveSouls()
        {
            var statsBasePtr = (IntPtr) _memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(GameDataMan.Base) + GameDataMan.PlayerGameData);
            int currentVal = _memoryIo.ReadInt32(statsBasePtr + (int)GameDataMan.Stats.Souls);
            HandleSoulEdit(statsBasePtr, currentVal + 10000, currentVal);
        }

        private int CalculateTotalSoulsRequired(int startLevel, int endLevel)
        {
            startLevel = Math.Max(1, startLevel);
            double totalSouls = 0;
            for (int level = startLevel + 1; level <= endLevel; level++)
            {
                if (level <= 11)
                {
                    totalSouls += _lowLevelSoulRequirements[level];
                }
                else
                {
                    double x = level;
                    double levelCost = 0.02 * Math.Pow(x, 3) + 3.06 * Math.Pow(x, 2) + 105.6 * x - 895;
                    totalSouls += Math.Round(levelCost);
                }
            }

            return (int)totalSouls;
        }

        public float GetPlayerSpeed() => _memoryIo.ReadFloat(GetPlayerSpeedPtr());

        public void SetPlayerSpeed(float speed) => _memoryIo.WriteFloat(GetPlayerSpeedPtr(), speed);

        private IntPtr GetPlayerSpeedPtr()
        {
            return _memoryIo.FollowPointers(WorldChrMan.Base,
                new[]
                {
                    WorldChrMan.PlayerIns,
                    (int)WorldChrMan.PlayerInsOffsets.Modules,
                    (int)WorldChrMan.Modules.ChrBehaviorModule,
                    (int)WorldChrMan.ChrBehaviorModule.AnimSpeed
                }, false);
        }


        public void ToggleNoRoll(bool isNoRollEnabled)
        {
            if (isNoRollEnabled)
            {
                _memoryIo.WriteByte(Patches.NoRoll + 0x6, 0);
                _memoryIo.WriteByte(Patches.NoRoll + 0x15, 0);
            }
            else
            {
                _memoryIo.WriteByte(Patches.NoRoll + 0x6, 1);
                _memoryIo.WriteByte(Patches.NoRoll + 0x15, 1);
            }
        }

        public int GetMp() => _memoryIo.ReadInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Mp));
        public int GetSp() => _memoryIo.ReadInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Stam));

        public void SetMp(int val) => _memoryIo.WriteInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Mp), val);
        public void SetSp(int val) => _memoryIo.WriteInt32(GetChrDataFieldPtr((int)WorldChrMan.ChrDataModule.Stam), val);
    }
}