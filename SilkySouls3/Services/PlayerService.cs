using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class PlayerService(
        IMemoryService memoryService,
        IChrInsService chrInsService,
        IReminderService reminderService,
        ITravelService travelService) : IPlayerService
    {
        private const int StatsBlockSize = 0x38;
        private const int CemeteryOfAshBlockId = 671088640;
        private const int UntendedGravesCeremonyId = 40000010;
        private const int UntendedGravesBonfireId = 4001953;
        private const int WorldBlockInfoSize = 0x70;

        private readonly Dictionary<int, int> _lowLevelSoulRequirements = new()
        {
            { 2, 673 }, { 3, 690 }, { 4, 707 }, { 5, 724 }, { 6, 741 }, { 7, 758 }, { 8, 775 }, { 9, 793 }, { 10, 811 },
            { 11, 829 },
        };

        private readonly Dictionary<uint, int> _bonfiresByBlockId = DataLoader.LoadDict<uint, int>("BonfiresByBlockId");

        private readonly Position[] _positions =
        [
            new(0, Vector3.Zero, 0f),
            new(0, Vector3.Zero, 0f)
        ];

        public nint GetPlayerIns() =>
            memoryService.Read<nint>(memoryService.Read<nint>(WorldChrManImp.Base) + WorldChrManImp.PlayerIns);

        public int GetHp() => chrInsService.GetCurrentHp(GetPlayerIns());

        public int GetMaxHp() => chrInsService.GetMaxHp(GetPlayerIns());

        public void SetHp(int hp) => chrInsService.SetHp(GetPlayerIns(), hp);

        public int GetMp() => chrInsService.GetMp(GetPlayerIns());

        public int GetMaxMp() => chrInsService.GetMaxMp(GetPlayerIns());

        public void SetMp(int mp) => chrInsService.SetMp(GetPlayerIns(), mp);

        public int GetSp() => chrInsService.GetSp(GetPlayerIns());

        public void SetSp(int sp) => chrInsService.SetSp(GetPlayerIns(), sp);

        public void ToggleNoDamage(bool isEnabled) => chrInsService.ToggleNoDamage(GetPlayerIns(), isEnabled);

        public float GetPlayerSpeed() => chrInsService.GetSpeed(GetPlayerIns());

        public void SetPlayerSpeed(float speed) => chrInsService.SetSpeed(GetPlayerIns(), speed);

        public int GetNewGame() =>
            memoryService.Read<int>(memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.PlayerGameDataOffsets.NewGame);

        public void SetNewGame(int value) =>
            memoryService.Write(memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.PlayerGameDataOffsets.NewGame, value);

        public void ToggleDebugFlag(int offset, int value) =>
            memoryService.Write(DebugFlags.Base + offset, (byte)value);

        public void ToggleInfinitePoise(bool isEnabled) => chrInsService.ToggleInfinitePoise(GetPlayerIns(), isEnabled);

        public void ToggleNoHit(bool isEnabled)
        {
            reminderService.TrySetReminder();
            chrInsService.ToggleNoHit(GetPlayerIns(), isEnabled);
        }

        public void SavePosition(int index)
        {
            var posToSave = _positions[index];
            var playerIns = GetPlayerIns();
            var currentBlockId = memoryService.Read<uint>(
                memoryService.FollowPointers(playerIns, ChrIns.CurrentBlockId, false));
            var physicsModule = memoryService.FollowPointers(playerIns, ChrIns.ChrPhysicsModule, true);

            if (currentBlockId == CemeteryOfAshBlockId)
            {
                posToSave.CeremonyId = ReadCeremonyId();
            }

            posToSave.BlockId = currentBlockId;
            posToSave.Coords = memoryService.Read<Vector3>(physicsModule + ChrIns.ChrPhysicsOffsets.Position);
            posToSave.Angle = memoryService.Read<float>(physicsModule + ChrIns.ChrPhysicsOffsets.Angle);
        }

        public void RestorePosition(int index)
        {
            var savedPos = _positions[index];

            if (IsInSavedLocation(savedPos))
            {
                var playerIns = GetPlayerIns();
                var wasNoDamageEnabled = chrInsService.IsNoDamageEnabled(playerIns);
                var wasNoDeathEnabled = memoryService.Read<byte>(DebugFlags.Base + DebugFlags.NoDeath) != 0;
                if (!wasNoDamageEnabled) chrInsService.ToggleNoDamage(playerIns, true);
                if (!wasNoDeathEnabled) ToggleDebugFlag(DebugFlags.NoDeath, 1);
                var physicsModule = memoryService.FollowPointers(playerIns, ChrIns.ChrPhysicsModule, true);
                memoryService.Write(physicsModule + ChrIns.ChrPhysicsOffsets.Position, savedPos.Coords);
                memoryService.Write(physicsModule + ChrIns.ChrPhysicsOffsets.Angle, savedPos.Angle);
                Thread.Sleep(500);
                if (!wasNoDamageEnabled) chrInsService.ToggleNoDamage(playerIns, false);
                if (!wasNoDeathEnabled) ToggleDebugFlag(DebugFlags.NoDeath, 0);
            }
            else
            {
                travelService.WarpWithCoords(savedPos.Coords, savedPos.Angle,
                    ResolveBonfireId(savedPos.BlockId, savedPos.CeremonyId));
            }
        }

        public void ForceSetPosition(Vector3 position) => chrInsService.ForceSetPosition(GetPlayerIns(), position);

        public int GetCurrentBlockId() => memoryService.Read<int>(
            memoryService.FollowPointers(GetPlayerIns(), ChrIns.CurrentBlockId, false));

        public Vector3 GetPosition() => chrInsService.GetPosition(GetPlayerIns());

        public void ToggleInfiniteDurability(bool isEnabled)
        {
            memoryService.Write(Patches.InfiniteDurability + 0x1,
                isEnabled ? (byte)0x84 : (byte)0x85);
        }

        public void ToggleNoGoodsConsume(bool isEnabled) =>
            chrInsService.ToggleNoGoodsConsume(GetPlayerIns(), isEnabled);

        public Stats GetStats()
        {
            const int statsStart = (int)GameDataMan.PlayerGameDataOffsets.Stats.Vigor;
            var block = new MemoryBlock(memoryService.ReadBytes(GetGameDataPtr() + statsStart, StatsBlockSize));

            return new Stats
            {
                Vigor = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Vigor - statsStart),
                Attunement = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Attunement - statsStart),
                Endurance = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Endurance - statsStart),
                Vitality = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Vitality - statsStart),
                Strength = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Strength - statsStart),
                Dexterity = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Dexterity - statsStart),
                Intelligence = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Intelligence - statsStart),
                Faith = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Faith - statsStart),
                Luck = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Luck - statsStart),
                SoulLevel = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.SoulLevel - statsStart),
                Souls = block.Get<int>((int)GameDataMan.PlayerGameDataOffsets.Stats.Souls - statsStart),
            };
        }

        public int GetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats stat) =>
            memoryService.Read<int>(GetGameDataPtr() + (int)stat);

        public void SetPlayerStat(GameDataMan.PlayerGameDataOffsets.Stats stat, int newValue)
        {
            var playerGameData = GetGameDataPtr();
            int currentVal = memoryService.Read<int>(playerGameData + (int)stat);
            if (currentVal == newValue) return;

            if (stat == GameDataMan.PlayerGameDataOffsets.Stats.Souls) HandleSoulEdit(playerGameData, newValue, currentVal);
            else HandleStatEdit(playerGameData, stat, newValue, currentVal);
        }

        public void GiveSouls()
        {
            var playerGameData = GetGameDataPtr();
            int currentVal = memoryService.Read<int>(playerGameData + (int)GameDataMan.PlayerGameDataOffsets.Stats.Souls);
            HandleSoulEdit(playerGameData, currentVal + 10000, currentVal);
        }

        public void ToggleNoRoll(bool isNoRollEnabled)
        {
            if (isNoRollEnabled)
            {
                memoryService.Write(Patches.NoRoll + 0x6, (byte)0);
                memoryService.Write(Patches.NoRoll + 0x15, (byte)0);
            }
            else
            {
                memoryService.Write(Patches.NoRoll + 0x6, (byte)1);
                memoryService.Write(Patches.NoRoll + 0x15, (byte)1);
            }
        }

        public void Rest()
        {
            var bytes = AsmLoader.GetAsmBytes(AsmScript.Rest);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
                (GetPlayerIns(), 0x0 + 2),
                (Functions.Rest, 0xF + 2)
            ]);
            memoryService.AllocateAndExecute(bytes);
        }

        #region Private Methods

        private void HandleSoulEdit(IntPtr statsBasePtr, int newValue, int currentVal)
        {
            if (newValue < currentVal)
            {
                memoryService.Write(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.Souls, newValue);
                return;
            }

            int difference = newValue - currentVal;
            int currentTotalSouls = memoryService.Read<int>(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.TotalSouls);
            memoryService.Write(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.TotalSouls, difference + currentTotalSouls);
            memoryService.Write(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.Souls, newValue);
        }

        private void HandleStatEdit(IntPtr statsBasePtr, GameDataMan.PlayerGameDataOffsets.Stats stat, int newValue, int currentVal)
        {
            var validatedStat = newValue;
            if (validatedStat < 1) validatedStat = 1;
            if (validatedStat > 99) validatedStat = 99;
            if (validatedStat == currentVal) return;
            memoryService.Write(statsBasePtr + (int)stat, newValue);

            int currentSoulLevel = memoryService.Read<int>(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.SoulLevel);
            int newLevel = currentSoulLevel + (validatedStat - currentVal);

            memoryService.Write(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.SoulLevel, newLevel);
            if (newLevel < currentSoulLevel) return;

            int totalSoulsRequired = CalculateTotalSoulsRequired(currentSoulLevel, newLevel);
            int currentTotalSouls = memoryService.Read<int>(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.TotalSouls);
            memoryService.Write(statsBasePtr + (int)GameDataMan.PlayerGameDataOffsets.Stats.TotalSouls,
                totalSoulsRequired + currentTotalSouls);
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

        private nint GetGameDataPtr() =>
            memoryService.Read<nint>(memoryService.Read<nint>(GameDataMan.Base) + GameDataMan.PlayerGameData);

        private int ReadCeremonyId()
        {
            var fieldArea = memoryService.Read<nint>(FieldArea.Base);
            var worldInfoOwner = memoryService.Read<nint>(fieldArea + FieldArea.WorldInfoOwnerPtr);
            var currentIdx = memoryService.Read<int>(fieldArea + FieldArea.CurrentBlockIdx);

            var blocks = memoryService.Read<nint>(worldInfoOwner + FieldArea.WorldInfoOwner.Blocks);
            var worldBlockInfo = blocks + currentIdx * WorldBlockInfoSize;
            return memoryService.Read<int>(worldBlockInfo + FieldArea.WorldBlockInfo.CeremonyId);
        }

        private bool IsInSavedLocation(Position saved)
        {
            var currentBlockId = GetCurrentBlockId();
            if (currentBlockId != saved.BlockId) return false;
            if (currentBlockId == CemeteryOfAshBlockId && ReadCeremonyId() != saved.CeremonyId) return false;
            return true;
        }

        private int ResolveBonfireId(uint blockId, int ceremonyId)
        {
            if (blockId == CemeteryOfAshBlockId && ceremonyId == UntendedGravesCeremonyId)
                return UntendedGravesBonfireId;
            return _bonfiresByBlockId[blockId];
        }

        #endregion
    }
}