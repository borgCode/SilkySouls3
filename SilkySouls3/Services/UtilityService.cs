using System;
using System.Collections.Generic;
using System.Threading;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class UtilityService
    {
        private readonly MemoryIo _memoryIo;
        private readonly HookManager _hookManager;

        public UtilityService(MemoryIo memoryIo, HookManager hookManager)
        {
            _memoryIo = memoryIo;
            _hookManager = hookManager;
        }

        public void Warp(WarpEntry warpLocation)
        {
            var bonfireFlagBasePtr = (IntPtr)_memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(EventFlagMan.Base));
            _memoryIo.SetBitValue(bonfireFlagBasePtr + EventFlagMan.CoiledSword, EventFlagMan.CoiledSwordBitFlag, true);

            var lastBonfireAddr = (IntPtr)_memoryIo.ReadInt64(GameMan.Base) + GameMan.LastBonfire;
            _memoryIo.WriteInt32(lastBonfireAddr, warpLocation.BonfireID + 1000);

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

        public void UnlockBonfires(IEnumerable<WarpEntry> warps)
        {
            var bonfireFlagBasePtr = (IntPtr)_memoryIo.ReadInt64((IntPtr)_memoryIo.ReadInt64(EventFlagMan.Base));
            _memoryIo.SetBitValue(bonfireFlagBasePtr + EventFlagMan.CoiledSword, EventFlagMan.CoiledSwordBitFlag, true);

            foreach (var warp in warps)
            {
                var addr = bonfireFlagBasePtr + warp.Offset.Value;
                byte flagMask = (byte)(1 << warp.BitPosition.Value);
                _memoryIo.SetBitValue(addr, flagMask, true);
            }
        }

        public void ToggleNoClip(bool isNoClipEnabled)
        {
            var inAirTimerCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.InAirTimerCode;
            var keyboardCheckCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.KeyboardCheck;
            var triggerCheckCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.TriggerCheck;
            var triggerCheckCode2 = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.TriggerCheck2;
            var updateCoordsCode = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.UpdateCoordsCode;
            if (isNoClipEnabled)
            {
                var zDirectionVariable = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.ZDirectionVariable;
                var triggerThreshold = CodeCaveOffsets.Base + (int)CodeCaveOffsets.NoClip.TriggerThreshold;
                _memoryIo.WriteFloat(triggerThreshold, 0.5f);

                var inAirTimerOrigin = Hooks.InAirTimer;
                var keyboardOrigin = Hooks.NoClipKeyboard;
                var triggerOrigin = Hooks.NoClipTriggers;
                var triggerOrigin2 = Hooks.NoClipTriggers2;
                var updateCoordsOrigin = Hooks.NoClipUpdateCoords;

                var playerCoordBase = _memoryIo.FollowPointers(WorldChrMan.Base,
                    new[]
                    {
                        WorldChrMan.PlayerIns,
                        (int)WorldChrMan.PlayerInsOffsets.UpdateCoords
                    }, true);

                var chrExFollowCam = _memoryIo.FollowPointers(FieldArea.Base, new[]
                {
                    FieldArea.ChrCam,
                    FieldArea.ChrExFollowCam
                }, true);

                var movementInfo = _memoryIo.FollowPointers(PadMan.Base, new[]
                {
                    PadMan.MovementInfoPtr,
                    PadMan.VirtualMultiDevice,
                    PadMan.MovementInfoPtr2
                }, true);

                var inAirTimerBytes = AsmLoader.GetAsmBytes("NoClip_InAirTimer");
                byte[] bytes = BitConverter.GetBytes(playerCoordBase.ToInt64());
                Array.Copy(bytes, 0, inAirTimerBytes, 0x1 + 2, bytes.Length);
                bytes = AsmHelper.GetJmpOriginOffsetBytes(inAirTimerOrigin, 8, inAirTimerCode + 0x25);
                Array.Copy(bytes, 0, inAirTimerBytes, 0x20 + 1, bytes.Length);
                _memoryIo.WriteBytes(inAirTimerCode, inAirTimerBytes);

                var keyboardCheckBytes = AsmLoader.GetAsmBytes("NoClip_Keyboard");
                AsmHelper.WriteJumpOffsets(keyboardCheckBytes, new[]
                {
                    (keyboardOrigin, 7, keyboardCheckCode + 0x1C, 0x17 + 1),
                    (keyboardOrigin, 7, keyboardCheckCode + 0x2A, 0x25 + 1),
                    (keyboardOrigin, 7, keyboardCheckCode + 0x38, 0x33 + 1)
                });

                AsmHelper.WriteRelativeOffsets(keyboardCheckBytes, new[]
                {
                    (keyboardCheckCode.ToInt64() + 0x1C, zDirectionVariable.ToInt64(), 7, 0x1C + 2),
                    (keyboardCheckCode.ToInt64() + 0x2A, zDirectionVariable.ToInt64(), 7, 0x2A + 2),
                });

                _memoryIo.WriteBytes(keyboardCheckCode, keyboardCheckBytes);

                var triggerBytes = AsmLoader.GetAsmBytes("NoClip_Triggers");

                bytes = AsmHelper.GetJmpOriginOffsetBytes(triggerOrigin, 6, triggerCheckCode + 0x20);
                Array.Copy(bytes, 0, triggerBytes, 0x1B + 1, 4);

                AsmHelper.WriteRelativeOffsets(triggerBytes, new[]
                {
                    (triggerCheckCode.ToInt64(), triggerThreshold.ToInt64(), 7, 0x0 + 3),
                    (triggerCheckCode.ToInt64() + 0x17, zDirectionVariable.ToInt64(), 7, 0x20 + 2),
                    (triggerCheckCode.ToInt64() + 0x2C, zDirectionVariable.ToInt64(), 7, 0x2C + 2),
                });

                _memoryIo.WriteBytes(triggerCheckCode, triggerBytes);

                bytes = AsmHelper.GetJmpOriginOffsetBytes(triggerOrigin2, 6, triggerCheckCode2 + 0x20);
                Array.Copy(bytes, 0, triggerBytes, 0x1B + 1, 4);

                AsmHelper.WriteRelativeOffsets(triggerBytes, new[]
                {
                    (triggerCheckCode2.ToInt64(), triggerThreshold.ToInt64(), 7, 0x0 + 3),
                    (triggerCheckCode2.ToInt64() + 0x17, zDirectionVariable.ToInt64(), 7, 0x20 + 2),
                    (triggerCheckCode2.ToInt64() + 0x2C, zDirectionVariable.ToInt64(), 7, 0x2C + 2)
                });

                _memoryIo.WriteBytes(triggerCheckCode2, triggerBytes);

                var updateCoordsBytes = AsmLoader.GetAsmBytes("NoClip_UpdateCoords");
                AsmHelper.WriteAbsoluteAddresses(updateCoordsBytes, new[]
                {
                    (playerCoordBase.ToInt64(), 0x1 + 2),
                    (movementInfo.ToInt64(), 0x22 + 2),
                    (chrExFollowCam.ToInt64(), 0x4D + 2),
                });

                AsmHelper.WriteRelativeOffsets(updateCoordsBytes, new[]
                {
                    (updateCoordsCode.ToInt64() + 0x90, zDirectionVariable.ToInt64(), 7, 0x90 + 2),
                    (updateCoordsCode.ToInt64() + 0xA0, zDirectionVariable.ToInt64(), 7, 0xA0 + 2),
                    (updateCoordsCode.ToInt64() + 0xBC, zDirectionVariable.ToInt64(), 7, 0xBC + 2),
                });

                bytes = AsmHelper.GetJmpOriginOffsetBytes(updateCoordsOrigin, 8, updateCoordsCode + 0xE2);
                Array.Copy(bytes, 0, updateCoordsBytes, 0xDD + 1, 4);

                _memoryIo.WriteBytes(updateCoordsCode, updateCoordsBytes);


                _hookManager.InstallHook(inAirTimerCode.ToInt64(), inAirTimerOrigin, new byte[]
                    { 0xF3, 0x0F, 0x11, 0x81, 0xB0, 0x01, 0x00, 0x00 });
                _hookManager.InstallHook(keyboardCheckCode.ToInt64(), keyboardOrigin, new byte[]
                    { 0x41, 0xFF, 0x90, 0xF8, 0x00, 0x00, 0x00 });
                _hookManager.InstallHook(triggerCheckCode.ToInt64(), triggerOrigin, new byte[]
                    { 0x48, 0x8B, 0x10, 0x48, 0x89, 0xC1 });
                _hookManager.InstallHook(triggerCheckCode2.ToInt64(), triggerOrigin2, new byte[]
                    { 0x48, 0x8B, 0x10, 0x48, 0x89, 0xC1 });
                _hookManager.InstallHook(updateCoordsCode.ToInt64(), updateCoordsOrigin, new byte[]
                    { 0x66, 0x0F, 0x7F, 0xB3, 0x80, 0x00, 0x00, 0x00 });
            }
            else
            {
                _hookManager.UninstallHook(updateCoordsCode.ToInt64());
                _hookManager.UninstallHook(triggerCheckCode2.ToInt64());
                _hookManager.UninstallHook(triggerCheckCode.ToInt64());
                _hookManager.UninstallHook(keyboardCheckCode.ToInt64());
                _hookManager.UninstallHook(inAirTimerCode.ToInt64());
            }
        }

        public void ToggleHitboxView(bool isHitboxEnabled) =>
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(DamageMan.Base) + DamageMan.HitboxView,
                isHitboxEnabled ? 1 : 0);

        public void ToggleSoundView(bool isSoundViewEnabled)
        {
            _memoryIo.WriteBytes(Patches.DebugFont, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 });
            _memoryIo.WriteByte(Patches.PlayerSoundView + 0x3,
                isSoundViewEnabled ? 1 : 0);
        }

        public void SetEvent(ulong flagId)
        {
            var eventMan = _memoryIo.ReadInt64(EventFlagMan.Base);
            var setEventBytes = AsmLoader.GetAsmBytes("SetEvent");
            var bytes = BitConverter.GetBytes(eventMan);
            Array.Copy(bytes, 0, setEventBytes, 0x2, 8);
            bytes = BitConverter.GetBytes(flagId);
            Array.Copy(bytes, 0, setEventBytes, 0xA + 2, 8);
            bytes = BitConverter.GetBytes(Funcs.SetEvent);
            Array.Copy(bytes, 0, setEventBytes, 0x24 + 2, 8);
            _memoryIo.AllocateAndExecute(setEventBytes);
        }

        public void SetMultipleEvents(params ulong[] flagIds)
        {
            foreach (var flagId in flagIds)
            {
                SetEvent(flagId);
            }
        }

        public void ToggleGroupMask(int offset, bool isEnabled) =>
            _memoryIo.WriteByte(GroupMask.Base + offset, isEnabled ? 0 : 1);


        public void ToggleTargetingView(bool isTargetingViewEnabled)
        {
            if (isTargetingViewEnabled)
            {
                _memoryIo.WriteByte(AiTargetingFlags.Base + AiTargetingFlags.Height, 1);
                _memoryIo.WriteByte(AiTargetingFlags.Base + AiTargetingFlags.Width, 1);
            }
            else
            {
                _memoryIo.WriteByte(AiTargetingFlags.Base + AiTargetingFlags.Height, 0);
                _memoryIo.WriteByte(AiTargetingFlags.Base + AiTargetingFlags.Width, 0);
            }
        }

        public void ToggleEventDraw(bool isDrawEventEnabled) =>
            _memoryIo.WriteByte((IntPtr)_memoryIo.ReadInt64(DebugEvent.Base) + DebugEvent.EventDraw,
                isDrawEventEnabled ? 1 : 0);
    }
}