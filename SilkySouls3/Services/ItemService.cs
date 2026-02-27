using System;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class ItemService(IMemoryService memoryService) : IItemService
    {
        public void SpawnItem(int itemId, int quantity, bool shouldAdjustQuantity, int maxQuantity)
        {
            var qtyAdjustFlag = CustomCodeOffsets.Base + (int)CustomCodeOffsets.ItemSpawn.QtyAdjustFlag;
            var maxQty = CustomCodeOffsets.Base + (int)CustomCodeOffsets.ItemSpawn.MaxQty;
            var itemToSpawn = CustomCodeOffsets.Base + (int)CustomCodeOffsets.ItemSpawn.ItemToSpawn;
            var param3Addr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.ItemSpawn.Param3Addr;
            var code = CustomCodeOffsets.Base + (int)CustomCodeOffsets.ItemSpawn.Code;

            byte[] itemBytes = new byte[16];
            BitConverter.GetBytes(1).CopyTo(itemBytes, 0);
            BitConverter.GetBytes(itemId).CopyTo(itemBytes, 4);
            BitConverter.GetBytes(quantity).CopyTo(itemBytes, 8);
            BitConverter.GetBytes(100).CopyTo(itemBytes, 12);

            memoryService.WriteBytes(itemToSpawn, itemBytes);
            memoryService.Write(qtyAdjustFlag, shouldAdjustQuantity);
            memoryService.Write(maxQty, maxQuantity);

            var codeBytes = AsmLoader.GetAsmBytes(AsmScript.ItemSpawn);
            AsmHelper.WriteRelativeOffsets(codeBytes, [
                (code + 0x4, itemToSpawn, 7, 0x4 + 3),
                (code + 0xB, qtyAdjustFlag, 7, 0xB + 2),
                (code + 0x22, Functions.GetItemQuantity, 5, 0x22 + 1),
                (code + 0x37, Functions.GetItemQuantity, 5, 0x37 + 1),
                (code + 0x3E, maxQty, 7, 0x3E + 3),
                (code + 0x5E, MapItemMan.Base, 7, 0x5E + 3),
                (code + 0x68, param3Addr, 7, 0x68 + 3),
                (code + 0x6F, Functions.ItemSpawn, 5, 0x6F + 1)
            ]);

            var equipInventory = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
                [GameDataMan.PlayerGameData, GameDataMan.PlayerGameDataOffsets.EquipInventory], false);
            var storageInventory = memoryService.FollowPointers(memoryService.Read<nint>(GameDataMan.Base),
                [GameDataMan.PlayerGameData, GameDataMan.PlayerGameDataOffsets.StorageInventory], true);

            AsmHelper.WriteAbsoluteAddresses(codeBytes, [
                (equipInventory, 0x14 + 2),
                (storageInventory, 0x29 + 2)
            ]);

            memoryService.WriteBytes(code, codeBytes);
            memoryService.RunThread(code);
        }
    }
}