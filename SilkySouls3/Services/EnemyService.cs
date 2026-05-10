using System;
using System.Text;
using SilkySouls3.Data;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class EnemyService(IMemoryService memoryService, HookManager hookManager) : IEnemyService
    {
        private const string PontiffNoCloneResourceName = "PontiffNoClone";
        private const string PontiffVanillaResourceName = "PontiffVanilla";
        
        
        public void ToggleDebugFlag(int offset, bool isEnabled) =>
            memoryService.Write(DebugFlags.Base + offset, isEnabled);

        public void ToggleAllRepeatAct(bool isEnabled) =>
            memoryService.Write(Patches.RepeatAct, isEnabled ? (byte)0x82 : (byte)0x81);

        public void ToggleTargetingView(bool isTargetingViewEnabled)
        {
            memoryService.Write(AiTargetingFlags.Base + AiTargetingFlags.Height, isTargetingViewEnabled ? (byte)1 : (byte)0);
            memoryService.Write(AiTargetingFlags.Base + AiTargetingFlags.Width, isTargetingViewEnabled ? (byte)1 : (byte)0);
        }

        public void PlacePrismStones()
        {
            var allocatedMem = memoryService.AllocateMem(0x1000);
            var transforms = allocatedMem + 0x480;
            memoryService.WriteBytes(transforms, PrismStoneData.TransformBytes);
            
            
            var code = allocatedMem + 0x800;
            var bytes = AsmLoader.GetAsmBytes(AsmScript.PlacePrismStones);
            
            AsmHelper.WriteRelativeOffsets(bytes, [
            (code + 0x4, transforms, 7, 0x4 + 3),
            (code + 0xD, allocatedMem, 7, 0xD + 3),
            (code + 0x38, allocatedMem, 7, 0x38 + 3),
            (code + 0x51, allocatedMem, 7, 0x51 + 3),
            ]);
            
            AsmHelper.WriteAbsoluteAddresses(bytes, [
            (Functions.SpawnSfxSimple, 0x22 + 2),
            (memoryService.Read<nint>(SprjBulletManager.Base), 0x2E + 2),
            (Functions.RegisterPrismStoneSfx, 0x45 + 2),
            (Functions.DestroyFxInner, 0x5C + 2)
            ]);
            
            memoryService.WriteBytes(code, bytes);
            memoryService.RunThread(code);
            memoryService.FreeMem(allocatedMem);
        }
        

        public void ToggleButterflyRng(bool isEnabled)
        {
            var code = CustomCodeOffsets.Base + (int)CustomCodeOffsets.Butterfly.Code;
            
            if (isEnabled)
            {
                var leftSideIdPtr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.Butterfly.LeftSideAnimationId;
                var rightSideIdPtr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.Butterfly.RightSideAnimationId;
                var hook = Hooks.AddSubGoalDsa;
            
                var bytes = AsmLoader.GetAsmBytes(AsmScript.ButterflyRng);
            
                AsmHelper.WriteRelativeOffsets(bytes, [
                    (code + 0x1F, leftSideIdPtr, 6, 0x1F + 2),
                    (code + 0x27, leftSideIdPtr, 8, 0x27 + 4),
                    (code + 0x34, rightSideIdPtr, 6, 0x34 + 2),
                    (code + 0x3C, rightSideIdPtr, 8, 0x3C + 4),
                    (code + 0x4F, hook + 0xA, 5, 0x4F + 1)
                ]);
                memoryService.WriteBytes(code, bytes);
                hookManager.InstallHook(code, hook, [
                    0x48, 0x89, 0xE0, 0x48, 0x81, 0xEC, 0x98, 0x00, 0x00, 0x00
                ]);
            }
            else
            {
                hookManager.UninstallHook(code);
            }
        }

        public void SetLeftButterflyAttack(float animationId)
        {
            var leftSideIdPtr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.Butterfly.LeftSideAnimationId;
            memoryService.Write(leftSideIdPtr, animationId);
        }

        public void SetRightButterflyAttack(float animationId)
        {
            var rightSideIdPtr = CustomCodeOffsets.Base + (int)CustomCodeOffsets.Butterfly.RightSideAnimationId;
            memoryService.Write(rightSideIdPtr, animationId);
        }

        public void TogglePontiffNoClone(bool isEnabled)
        {
            if (isEnabled) InjectScript(PontiffNoCloneResourceName);
            else InjectScript(PontiffVanillaResourceName);
        }

        public void ToggleDrawNavigation(bool isEnabled) => memoryService.Write(DrawNavigationPath, isEnabled);

        private void InjectScript(string resourceName)
        {
            var content = Properties.Resources.ResourceManager.GetString(resourceName)
                          ?? throw new ArgumentException($"Resource '{resourceName}' not found.");
            var scriptBytes = Encoding.UTF8.GetBytes(content.Replace("\r\n", "\n") + '\0');
            
            var scriptPtr = memoryService.AllocateMem((uint) scriptBytes.Length);
            memoryService.WriteBytes(scriptPtr, scriptBytes);

            var luaState = memoryService.FollowPointers(memoryService.Read<nint>(WorldAiManager.Base),
                WorldAiManager.LuaState, true);

            var bytes = AsmLoader.GetAsmBytes(AsmScript.LuaDoString);
            AsmHelper.WriteAbsoluteAddresses(bytes, [
            (luaState, 2),
            (scriptPtr, 0xA + 2),
            (Functions.LuaDoString, 0x14 + 2)
            ]);
            memoryService.AllocateAndExecute(bytes);
            memoryService.FreeMem(scriptPtr);
        }
    }
}