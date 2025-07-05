using System;
using System.Threading;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class TravelService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;
        public TravelService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void Warp(WarpLocation warpLocation)
        {
            var bonfireFlagBasePtr = (IntPtr)_memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(EventFlagMan.Base));
            _memoryIo.SetBitValue(bonfireFlagBasePtr + EventFlagMan.CoiledSword, EventFlagMan.CoiledSwordBitFlag, true);

            var lastBonfireAddr = (IntPtr)_memoryIo.ReadInt64(GameMan.Base) + GameMan.LastBonfire;
            _memoryIo.WriteInt32(lastBonfireAddr, warpLocation.BonfireId + 1000);

            byte[] warpBytes = AsmLoader.GetAsmBytes("WarpCall");
            byte[] bytes = BitConverter.GetBytes(LuaEventMan.Base.ToInt64());
            Array.Copy(bytes, 0, warpBytes, 2, 8);
            bytes = BitConverter.GetBytes(Funcs.Warp);
            Array.Copy(bytes, 0, warpBytes, 0x12 + 2, 8);

            _memoryIo.AllocateAndExecute(warpBytes);

            if (warpLocation.HasCoordinates)
            {
                var coordsAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpHooks.Coords;
                var coordsOrigin = Hooks.WarpCoordWrite;
                var coordsCustomCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpHooks.CoordCode;

                var coords = new byte[4 * sizeof(float)];
                Buffer.BlockCopy(warpLocation.Coords, 0, coords, 0, 3 * sizeof(float));
                BitConverter.GetBytes(1f).CopyTo(coords, 12);

                _memoryIo.WriteBytes(coordsAddr, coords);

                byte[] customCode = AsmLoader.GetAsmBytes("WarpHookCode");
                bytes = AsmHelper.GetRelOffsetBytes(coordsCustomCode, coordsAddr, 0x8);
                Array.Copy(bytes, 0, customCode, 0x0 + 4, 4);
                bytes = AsmHelper.GetJmpOriginOffsetBytes(coordsOrigin, 8, coordsCustomCode + 0x19);
                Array.Copy(bytes, 0, customCode, 0x14 + 1, 4);
                _memoryIo.WriteBytes(coordsCustomCode, customCode);

                var angleAddr = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpHooks.Angle;
                var angleOrigin = Hooks.WarpCoordWrite - 0x20;
                var angleCustomCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.WarpHooks.AngleCode;

                byte[] angle = new byte[16];
                bytes = BitConverter.GetBytes(warpLocation.Angle);
                Array.Copy(bytes, 0, angle, 4, 4);
                _memoryIo.WriteBytes(angleAddr, angle);

                customCode[0xC] = 0x50;
                bytes = AsmHelper.GetRelOffsetBytes(angleCustomCode, angleAddr, 0x8);
                Array.Copy(bytes, 0, customCode, 0x0 + 4, 4);
                bytes = AsmHelper.GetJmpOriginOffsetBytes(angleOrigin, 8, angleCustomCode + 0x19);
                Array.Copy(bytes, 0, customCode, 0x14 + 1, 4);
                _memoryIo.WriteBytes(angleCustomCode, customCode);

                {
                    int start = Environment.TickCount;
                    while (_memoryIo.IsGameLoaded() && Environment.TickCount - start < 10000)
                        Thread.Sleep(50);
                }

                _hookManager.InstallHook(coordsCustomCode.ToInt64(), coordsOrigin,
                    new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x40, 0x0A, 0x00, 0x00 });
                _hookManager.InstallHook(angleCustomCode.ToInt64(), angleOrigin,
                    new byte[] { 0x66, 0x0F, 0x7F, 0x80, 0x50, 0x0A, 0x00, 0x00 });


                {
                    int start = Environment.TickCount;
                    while (!_memoryIo.IsGameLoaded() && Environment.TickCount - start < 10000)
                        Thread.Sleep(50);
                }

                _hookManager.UninstallHook(coordsCustomCode.ToInt64());
                _hookManager.UninstallHook(angleCustomCode.ToInt64());
            }
        }
    }
}