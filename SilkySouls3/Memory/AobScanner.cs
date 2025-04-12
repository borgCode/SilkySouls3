using System;
using System.Collections.Generic;
using System.Linq;

namespace SilkySouls3.Memory
{
    public class AoBScanner
    {
        private readonly MemoryIo _memoryIo;

        public AoBScanner(MemoryIo memoryIo)
        {
            _memoryIo = memoryIo;
        }
        
        public void Scan()
        {
            Offsets.WorldChrMan.Base = FindAddressByPattern(Patterns.WorldChrMan);
            Offsets.GameMan.Base = FindAddressByPattern(Patterns.GameMan);
            Offsets.LuaEventMan.Base = FindAddressByPattern(Patterns.LuaEventMan);
            Offsets.SoloParamRepo.Base = FindAddressByPattern(Patterns.SoloParamRepo);
            Offsets.AiTargetingFlags.Base = FindAddressByPattern(Patterns.AiTargetingFlags);
            Offsets.WorldAiMan.Base = FindAddressByPattern(Patterns.WorldAiMan);
            Offsets.EventFlagMan.Base = FindAddressByPattern(Patterns.EventFlagMan);
            Offsets.MenuMan.Base = FindAddressByPattern(Patterns.MenuMan);
            Offsets.DebugFlags.Base = FindAddressByPattern(Patterns.DebugFlags);
            Offsets.DebugEvent.Base = FindAddressByPattern(Patterns.DebugEvent);
            Offsets.MapItemMan.Base = FindAddressByPattern(Patterns.MapItemMan);
            Offsets.ResistGaugeMenuMan.Base = FindAddressByPattern(Patterns.ResistGaugeMenuMan);
            Offsets.GameDataMan.Base = FindAddressByPattern(Patterns.GameDataMan);
            Offsets.PadMan.Base = FindAddressByPattern(Patterns.PadMan);

            Offsets.Patches.NoLogo = FindAddressByPattern(Patterns.NoLogo);
            Offsets.Patches.RepeatAct = FindAddressByPattern(Patterns.RepeatAct);
            Offsets.Patches.GameSpeed = FindAddressByPattern(Patterns.GameSpeed);
            
            Offsets.Hooks.LastLockedTarget = FindAddressByPattern(Patterns.LockedTarget).ToInt64();
            Offsets.Hooks.WarpCoordWrite = FindAddressByPattern(Patterns.WarpCoordWrite).ToInt64();
            Offsets.Hooks.AddSubGoal = FindAddressByPattern(Patterns.AddSubGoal).ToInt64();
            Offsets.Hooks.InAirTimer = FindAddressByPattern(Patterns.NoClipInAirTimer).ToInt64();
            Offsets.Hooks.NoClipKeyboard = FindAddressByPattern(Patterns.NoClipKeyboard).ToInt64();
            var multiple = FindAddressesByPattern(Patterns.NoClipTriggers, 2);
            Offsets.Hooks.NoClipTriggers = multiple[0].ToInt64();
            Offsets.Hooks.NoClipTriggers2 = multiple[1].ToInt64();
            Offsets.Hooks.NoClipUpdateCoords = FindAddressByPattern(Patterns.NoClipUpdateCoords).ToInt64();

            Offsets.Funcs.Warp = FindAddressByPattern(Patterns.WarpFunc).ToInt64();
            Offsets.Funcs.ItemSpawn = FindAddressByPattern(Patterns.ItemSpawnFunc).ToInt64();
            
            Console.WriteLine($"WorldChrMan.Base: 0x{Offsets.WorldChrMan.Base.ToInt64():X}");
            Console.WriteLine($"GameMan.Base: 0x{Offsets.GameMan.Base.ToInt64():X}");
            Console.WriteLine($"LuaEventMan.Base: 0x{Offsets.LuaEventMan.Base.ToInt64():X}");
            Console.WriteLine($"EventFlagMan.Base: 0x{Offsets.EventFlagMan.Base.ToInt64():X}");
            Console.WriteLine($"SoloParamRepo.Base: 0x{Offsets.SoloParamRepo.Base.ToInt64():X}");
            Console.WriteLine($"AiTargetingFlags.Base: 0x{Offsets.AiTargetingFlags.Base.ToInt64():X}");
            Console.WriteLine($"WorldAiMan.Base: 0x{Offsets.WorldAiMan.Base.ToInt64():X}");
            Console.WriteLine($"MenuMan.Base: 0x{Offsets.MenuMan.Base.ToInt64():X}");
            Console.WriteLine($"DebugFlags.Base: 0x{Offsets.DebugFlags.Base.ToInt64():X}");
            Console.WriteLine($"DebugEvent.Base: 0x{Offsets.DebugEvent.Base.ToInt64():X}");
            Console.WriteLine($"MapItemMan.Base: 0x{Offsets.MapItemMan.Base.ToInt64():X}");
            Console.WriteLine($"ResistGaugeMenuMan.Base: 0x{Offsets.ResistGaugeMenuMan.Base.ToInt64():X}");
            Console.WriteLine($"GameDataMan.Base: 0x{Offsets.GameDataMan.Base.ToInt64():X}");
            Console.WriteLine($"PadMan.Base: 0x{Offsets.PadMan.Base.ToInt64():X}");
            
            
            
            Console.WriteLine($"Patches.NoLogo: 0x{Offsets.Patches.NoLogo.ToInt64():X}");
            Console.WriteLine($"Patches.RepeatAct: 0x{Offsets.Patches.RepeatAct.ToInt64():X}");
            Console.WriteLine($"Patches.GameSpeed: 0x{Offsets.Patches.GameSpeed.ToInt64():X}");
         
            
            // Console.WriteLine($"DebugFlags.Base: 0x{Offsets.DebugFlags.Base.ToInt64():X}");
            // Console.WriteLine($"Cam.Base: 0x{Offsets.Cam.Base.ToInt64():X}");
            // Console.WriteLine($"GameDataMan.Base: 0x{Offsets.GameDataMan.Base.ToInt64():X}");
            // Console.WriteLine($"ItemGet: 0x{Offsets.ItemGet:X}");
            // Console.WriteLine($"ItemGetMenuMan: 0x{Offsets.ItemGetMenuMan.ToInt64():X}");
            // Console.WriteLine($"ItemDlgFunc: 0x{Offsets.ItemDlgFunc:X}");
            // Console.WriteLine($"FieldArea.Base: 0x{Offsets.FieldArea.Base.ToInt64():X}");
            // Console.WriteLine($"GameMan.Base: 0x{Offsets.GameMan.Base.ToInt64():X}");
            // Console.WriteLine($"DamageMan.Base: 0x{Offsets.DamageMan.Base.ToInt64():X}");
            // Console.WriteLine($"DrawEventPatch: 0x{Offsets.DrawEventPatch.ToInt64():X}");
            // Console.WriteLine($"DrawSoundViewPatch: 0x{Offsets.DrawSoundViewPatch.ToInt64():X}");
            // Console.WriteLine($"MenuMan.Base: 0x{Offsets.MenuMan.Base.ToInt64():X}");
            // Console.WriteLine($"EventFlagMan.Base: 0x{Offsets.EventFlagMan.Base.ToInt64():X}");
            // Console.WriteLine($"LevelUpFunc: 0x{Offsets.LevelUpFunc:X}");
            // Console.WriteLine($"RestoreCastsFunc: 0x{Offsets.RestoreCastsFunc:X}");
            // Console.WriteLine($"HgDraw.Base: 0x{Offsets.HgDraw.Base.ToInt64():X}");
            // Console.WriteLine($"WarpEvent: 0x{Offsets.WarpEvent.ToInt64():X}");
            // Console.WriteLine($"WarpFunc: 0x{Offsets.WarpFunc:X}");
            // Console.WriteLine($"FastQuitout: 0x{Offsets.QuitoutPatch.ToInt64():X}");
            // Console.WriteLine($"WorldAiMan: 0x{Offsets.WorldAiMan.Base.ToInt64():X}");
            //
            // Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopWeapon:X}");
            // Console.WriteLine($"Weapon: 0x{Offsets.OpenEnhanceShopArmor:X}");
            //
            Console.WriteLine($"Hooks.LastLockedTarget: 0x{Offsets.Hooks.LastLockedTarget:X}");
            Console.WriteLine($"Hooks.WarpCoordWrite: 0x{Offsets.Hooks.WarpCoordWrite:X}");
            Console.WriteLine($"Hooks.AddSubGoal: 0x{Offsets.Hooks.AddSubGoal:X}");
            Console.WriteLine($"Hooks.InAirTimer: 0x{Offsets.Hooks.InAirTimer:X}");
            Console.WriteLine($"Hooks.NoClipKeyboard: 0x{Offsets.Hooks.NoClipKeyboard:X}");
            Console.WriteLine($"Hooks.NoClipTriggers: 0x{Offsets.Hooks.NoClipTriggers:X}");
            Console.WriteLine($"Hooks.NoClipTriggers2: 0x{Offsets.Hooks.NoClipTriggers2:X}");
            Console.WriteLine($"Hooks.NoClipUpdateCoords: 0x{Offsets.Hooks.NoClipUpdateCoords:X}");
           
           
            
            // Console.WriteLine($"Hooks.AllNoDamage: 0x{Offsets.Hooks.AllNoDamage:X}");
            // Console.WriteLine($"Hooks.ItemSpawn: 0x{Offsets.Hooks.ItemSpawn:X}");
            // Console.WriteLine($"Hooks.Draw: 0x{Offsets.Hooks.Draw:X}");
            // Console.WriteLine($"Hooks.TargetingView: 0x{Offsets.Hooks.TargetingView:X}");
            // Console.WriteLine($"Hooks.InAirTimer: 0x{Offsets.Hooks.InAirTimer:X}");
            // Console.WriteLine($"Hooks.Keyboard: 0x{Offsets.Hooks.Keyboard:X}");
            // Console.WriteLine($"Hooks.ControllerR2: 0x{Offsets.Hooks.ControllerR2:X}");
            // Console.WriteLine($"Hooks.ControllerL2: 0x{Offsets.Hooks.ControllerL2:X}");
            // Console.WriteLine($"Hooks.UpdateCoords: 0x{Offsets.Hooks.UpdateCoords:X}");
            // Console.WriteLine($"Hooks.WarpCoords: 0x{Offsets.Hooks.WarpCoords:X}");
            // Console.WriteLine($"Hooks.LuaIfElse: 0x{Offsets.Hooks.LuaIfCase:X}");
            Console.WriteLine($"Funcs.Warp: 0x{Offsets.Funcs.Warp:X}");
            Console.WriteLine($"Funcs.ItemSpawn: 0x{Offsets.Funcs.ItemSpawn:X}");
        }
        
        
        public IntPtr FindAddressByPattern(Pattern pattern)
        {
            var results = FindAddressesByPattern(pattern, 1);
            return results.Count > 0 ? results[0] : IntPtr.Zero;
        }
        
        public List<IntPtr> FindAddressesByPattern(Pattern pattern, int size)
        {
            List<IntPtr> addresses = PatternScanMultiple(pattern.Bytes, pattern.Mask, size);

            for (int i = 0; i < addresses.Count; i++)
            {
                IntPtr instructionAddress = IntPtr.Add(addresses[i], pattern.InstructionOffset);

                switch (pattern.RipType)
                {
                    case RipType.None:
                        addresses[i] = instructionAddress;
                        break;
                    case RipType.Mov64:
                        int stdOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                        addresses[i] = IntPtr.Add(instructionAddress, stdOffset + 7);
                        break;
                    case RipType.Mov32:
                        int mov32Offset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                        addresses[i] = IntPtr.Add(instructionAddress, mov32Offset + 6);
                        break;
                    case RipType.Cmp:
                        int cmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                        addresses[i] = IntPtr.Add(instructionAddress, cmpOffset + 7);
                        break;
                    case RipType.QwordCmp:
                        int qwordCmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                        addresses[i] = IntPtr.Add(instructionAddress, qwordCmpOffset + 8);
                        break;
                    case RipType.Call:
                        int callOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 1));
                        addresses[i] = IntPtr.Add(instructionAddress, callOffset + 5);
                        break;
                }
            }

            return addresses;
        }
        private List<IntPtr> PatternScanMultiple(byte[] pattern, string mask, int size)
        {
            const int chunkSize = 4096 * 16;
            byte[] buffer = new byte[chunkSize];

            IntPtr currentAddress = _memoryIo.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);

            List<IntPtr> addresses = new List<IntPtr>();

            while (currentAddress.ToInt64() < endAddress.ToInt64())
            {
                int bytesRemaining = (int)(endAddress.ToInt64() - currentAddress.ToInt64());
                int bytesToRead = Math.Min(bytesRemaining, buffer.Length);

                if (bytesToRead < pattern.Length)
                    break;

                buffer = _memoryIo.ReadBytes(currentAddress, bytesToRead);

                for (int i = 0; i <= bytesToRead - pattern.Length; i++)
                {
                    bool found = true;

                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (j < mask.Length && mask[j] == '?')
                            continue;

                        if (buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        addresses.Add(IntPtr.Add(currentAddress, i));
                    if (addresses.Count == size) break;
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }

            return addresses;
        }
    }
}