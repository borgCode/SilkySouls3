using System;
using SilkySouls3.Memory;

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
            _memoryIo.ReadInt32(GetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.Hp));

        public int GetMaxHp() =>
            _memoryIo.ReadInt32(GetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.MaxHp));
        
        public void SetHp(int hp) => 
            _memoryIo.WriteInt32(GetChrDataFieldPtr((int)Offsets.WorldChrMan.ChrDataModule.Hp), hp);
        
        private IntPtr GetChrDataFieldPtr(int fieldOffset)
        {
            return _memoryIo.FollowPointers(Offsets.WorldChrMan.Base,
                new[]
                {
                    Offsets.WorldChrMan.PlayerIns,
                    (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                    (int)Offsets.WorldChrMan.Modules.ChrDataModule,
                    fieldOffset
                }, false);
        }
        
        public void SavePos(int index)
        {
            var chrPhysicsModule = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, new[]
            {
                Offsets.WorldChrMan.PlayerIns,
                (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                (int)Offsets.WorldChrMan.Modules.ChrPhysicsModule,
            }, true);

            byte[] positionBytes = _memoryIo.ReadBytes(chrPhysicsModule + (int) Offsets.WorldChrMan.ChrPhysicsModule.Coords, 12);
            float angle = _memoryIo.ReadFloat(chrPhysicsModule + (int) Offsets.WorldChrMan.ChrPhysicsModule.Angle);

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
            
            var chrPhysicsModule = _memoryIo.FollowPointers(Offsets.WorldChrMan.Base, new[]
            {
                Offsets.WorldChrMan.PlayerIns,
                (int)Offsets.WorldChrMan.PlayerInsOffsets.Modules,
                (int)Offsets.WorldChrMan.Modules.ChrPhysicsModule,
            }, true);
            
            byte[] xyzBytes = new byte[12];
            Buffer.BlockCopy(positionBytes, 0, xyzBytes, 0, 12);
            
            _memoryIo.WriteBytes(chrPhysicsModule + (int)Offsets.WorldChrMan.ChrPhysicsModule.Coords, xyzBytes);
            _memoryIo.WriteFloat(chrPhysicsModule + (int)Offsets.WorldChrMan.ChrPhysicsModule.Angle, angle);
        }
        
        

        public void ToggleDebugFlag(int offset, int value)
        {
            _memoryIo.WriteByte(Offsets.DebugFlags.Base + offset, value);
        }
    }
}