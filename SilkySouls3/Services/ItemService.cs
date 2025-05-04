using System;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;

namespace SilkySouls3.Services
{
    public class ItemService
    {
        
        private readonly MemoryIo _memoryIo;
        public ItemService(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }

        public void SpawnItem(int itemId, int quantity)
        {
            var spawnFunc = Offsets.Funcs.ItemSpawn;
            var mapItemMan = Offsets.MapItemMan.Base;
            var itemToSpawn = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.ItemToSpawn;
            var param3Addr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.Param3Addr;
            var code = CodeCaveOffsets.Base + (int)CodeCaveOffsets.ItemSpawn.Code;
            

            byte[] itemBytes = new byte[16];
            BitConverter.GetBytes(1).CopyTo(itemBytes, 0);         
            BitConverter.GetBytes(itemId).CopyTo(itemBytes, 4);    
            BitConverter.GetBytes(quantity).CopyTo(itemBytes, 8);  
            BitConverter.GetBytes(100).CopyTo(itemBytes, 12); 
            
            _memoryIo.WriteBytes(itemToSpawn, itemBytes);

            byte[] itemSpawnBytes = AsmLoader.GetAsmBytes("ItemSpawn");
            AsmHelper.WriteRelativeOffsets(itemSpawnBytes, new[]
            {
                (code.ToInt64(), mapItemMan.ToInt64(), 7, 3),
                (code.ToInt64() + 0x7, param3Addr.ToInt64(), 7, 0x7 + 3),
                (code.ToInt64() + 0xE, itemToSpawn.ToInt64(), 7, 0xE + 3),
                (code.ToInt64() + 0x19, spawnFunc, 5, 0x19 + 1)
            });
            
            _memoryIo.WriteBytes(code, itemSpawnBytes);
            _memoryIo.RunThread(code);
        }
        
    }
}