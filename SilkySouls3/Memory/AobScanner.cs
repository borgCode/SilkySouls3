using System;

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

            Offsets.Patches.NoLogo = FindAddressByPattern(Patterns.NoLogo);
            
            
            Offsets.Hooks.LastLockedTarget = FindAddressByPattern(Patterns.LockedTarget).ToInt64();
            Offsets.Hooks.WarpCoordWrite = FindAddressByPattern(Patterns.WarpCoordWrite).ToInt64();
            Offsets.Hooks.AddSubGoal = FindAddressByPattern(Patterns.AddSubGoal).ToInt64();
            Offsets.Hooks.RepeatAct = FindAddressByPattern(Patterns.RepeatAct).ToInt64();

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
            
            
            
            Console.WriteLine($"Patches.NoLogo: 0x{Offsets.Patches.NoLogo.ToInt64():X}");
            
         
            
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
            Console.WriteLine($"Hooks.RepeatAct: 0x{Offsets.Hooks.RepeatAct:X}");
            
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
            IntPtr patternAddress = PatternScan(pattern.Bytes, pattern.Mask);
            if (patternAddress == IntPtr.Zero)
                return IntPtr.Zero;

            IntPtr instructionAddress = IntPtr.Add(patternAddress, pattern.InstructionOffset);

            switch (pattern.RipType)
            {
                case RipType.None:
                    return instructionAddress;

                case RipType.Mov64:
                    // e.g. 48 8B 05/0D - Standard mov rax/rcx,[rip+offset]
                    int stdOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, stdOffset + 7);

                case RipType.Mov32:
                    // e.g. 8B 05 - 32-bit mov eax,[rip+offset]
                    int mov32Offset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                    return IntPtr.Add(instructionAddress, mov32Offset + 6);
                
                case RipType.Cmp:
                    // e.g. 80 3D - cmp byte ptr [rip+offset],imm
                    int cmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 2));
                    return IntPtr.Add(instructionAddress, cmpOffset + 7);
                case RipType.QwordCmp:
                    // e.g. 48 83 3D - cmp qword ptr [rip+offset],imm
                    int qwordCmpOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 3));
                    return IntPtr.Add(instructionAddress, qwordCmpOffset + 8);
                case RipType.Call:
                    int callOffset = _memoryIo.ReadInt32(IntPtr.Add(instructionAddress, 1));
                    return IntPtr.Add(instructionAddress, callOffset + 5);

                default:
                    return IntPtr.Zero;
            }
        }
        
        public IntPtr PatternScan(byte[] pattern, string mask)
        {
            const int chunkSize = 4096 * 16;
            byte[] buffer = new byte[chunkSize];

            IntPtr currentAddress = _memoryIo.BaseAddress;
            IntPtr endAddress = IntPtr.Add(currentAddress, 0x3200000);

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
                        return IntPtr.Add(currentAddress, i);
                }

                currentAddress = IntPtr.Add(currentAddress, bytesToRead - pattern.Length + 1);
            }

            return IntPtr.Zero;
        }
        
    }
}