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
    
    public class TravelService(IMemoryService memoryService, HookManager hookManager) : ITravelService
    {
        private const int UntendedGravesBonfireId = 4001953;
        private const int ChampGundyrBonfireId = 4001954;
        
        public void Warp(int bonfireId)
        {
            var variant = bonfireId is UntendedGravesBonfireId or ChampGundyrBonfireId ? 10 : 0;
            
            var bytes = AsmLoader.GetAsmBytes(AsmScript.BonfireWarp);
            AsmHelper.WriteImmediateDwords(bytes, [
                (bonfireId, 1),
                (variant, 0x5 + 2)
            ]);
            AsmHelper.WriteAbsoluteAddress(bytes, Functions.BonfireWarp, 0xB + 2);
            memoryService.AllocateAndExecute(bytes);
        }

        public void WarpWithCoords(Vector3 coords, float angle, int bonfireId)
        {
            Warp(bonfireId);

            var coordsAddr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.WarpHooks.Coords;
            var coordsCode = CustomCodeOffsets.Base + (int)CustomCodeOffsets.WarpHooks.CoordCode;

            memoryService.Write(coordsAddr, coords);
            memoryService.Write(coordsAddr + 12, 1.0f);

            byte[] bytes = AsmLoader.GetAsmBytes(AsmScript.WarpCoordsWrite);
            AsmHelper.WriteRelativeOffsets(bytes, [
                (coordsCode, coordsAddr, 8, 4),
                (coordsCode + 0x14, Hooks.WarpCoords + 8, 5, 0x14 + 1)
            ]);

            memoryService.WriteBytes(coordsCode, bytes);

            var angleAddr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.WarpHooks.Angle;
            var angleCode = CustomCodeOffsets.Base + (int)CustomCodeOffsets.WarpHooks.AngleCode;

            memoryService.Write(angleAddr, 0L);
            memoryService.Write(angleAddr + 4, angle);
            memoryService.Write(angleAddr + 8, 0L);

            bytes = AsmLoader.GetAsmBytes(AsmScript.WarpAngleWrite);

            AsmHelper.WriteRelativeOffsets(bytes, [
                (angleCode, angleAddr, 8, 4),
                (angleCode + 0x14, Hooks.WarpAngle + 8, 5, 0x14 + 1)
            ]);

            memoryService.WriteBytes(angleCode, bytes);

            {
                int start = Environment.TickCount;
                while (IsLoaded() && Environment.TickCount - start < 10000)
                    Thread.Sleep(50);
            }

            hookManager.InstallHook(coordsCode, Hooks.WarpCoords,
                [0x66, 0x0F, 0x7F, 0x80, 0x40, 0x0A, 0x00, 0x00]);
            hookManager.InstallHook(angleCode, Hooks.WarpAngle,
                [0x66, 0x0F, 0x7F, 0x80, 0x50, 0x0A, 0x00, 0x00]);


            {
                int start = Environment.TickCount;
                while (!IsLoaded() && Environment.TickCount - start < 10000)
                    Thread.Sleep(50);
            }

            hookManager.UninstallHook(coordsCode);
            hookManager.UninstallHook(angleCode);
        }

        public void UnlockBonfires(IEnumerable<WarpLocation> warps)
        {
            var bonfireFlagBasePtr = memoryService.Read<nint>(memoryService.Read<nint>(EventFlagMan.Base));
            memoryService.SetBitValue(bonfireFlagBasePtr + EventFlagMan.CoiledSword, EventFlagMan.CoiledSwordBitFlag,
                true);

            foreach (var warp in warps)
            {
                var addr = bonfireFlagBasePtr + warp.Offset.Value;
                byte flagMask = (byte)(1 << warp.BitPosition.Value);
                memoryService.SetBitValue(addr, flagMask, true);
            }
        }

        private bool IsLoaded() =>
            memoryService.Read<nint>(memoryService.Read<nint>(WorldChrManImp.Base) + WorldChrManImp.PlayerIns) !=
            IntPtr.Zero;
    }
}