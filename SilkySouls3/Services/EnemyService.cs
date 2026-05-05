using System;
using System.Collections.Generic;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services
{
    public class EnemyService(IMemoryService memoryService, HookManager hookManager) : IEnemyService
    {
        private static readonly (float X, float Y, float Z)[] PrismStonePoints =
        {
            (-378.1000f, -55.3100f, -281.8500f),
            (-377.5936f, -55.3100f, -281.2373f),
            (-377.1380f, -55.3100f, -280.5880f),
            (-376.6544f, -55.3100f, -279.9588f),
            (-376.1760f, -55.3100f, -279.3260f),
            (-375.7240f, -55.3100f, -278.6740f),
            (-375.2456f, -55.3100f, -278.0412f),
            (-374.7620f, -55.3100f, -277.4120f),
            (-374.3064f, -55.3100f, -276.7627f),
            (-373.8000f, -55.3100f, -276.1500f),
            (-380.0000f, -55.3100f, -279.4500f),
            (-379.6625f, -55.3100f, -278.7374f),
            (-379.3353f, -55.3100f, -278.0206f),
            (-379.0167f, -55.3100f, -277.3001f),
            (-378.6706f, -55.3100f, -276.5911f),
            (-378.3416f, -55.3100f, -275.8749f),
            (-378.0058f, -55.3100f, -275.1617f),
            (-377.6942f, -55.3100f, -274.4383f),
            (-377.3411f, -55.3100f, -273.7322f),
            (-377.0208f, -55.3100f, -273.0125f),
            (-376.6764f, -55.3100f, -272.3028f),
            (-376.3647f, -55.3100f, -271.5794f),
            (-376.0117f, -55.3100f, -270.8733f),
            (-375.7000f, -55.3100f, -270.1500f),
        };
        
        private static readonly byte[] PrismStoneTransformBytes = CreatePrismStoneTransformBytes();

        private static byte[] CreatePrismStoneTransformBytes()
        {
            var buf = new byte[PrismStonePoints.Length * 0x20];

            for (var i = 0; i < PrismStonePoints.Length; i++)
            {
                var off = i * 0x20;
                var (x, y, z) = PrismStonePoints[i];

                BitConverter.GetBytes(x).CopyTo(buf, off + 0x00);
                BitConverter.GetBytes(y).CopyTo(buf, off + 0x04);
                BitConverter.GetBytes(z).CopyTo(buf, off + 0x08);
                BitConverter.GetBytes(1.0f).CopyTo(buf, off + 0x1C);
            }

            return buf;
        }
        
        
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
            memoryService.WriteBytes(transforms, PrismStoneTransformBytes);
            
            
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

        
    }
}