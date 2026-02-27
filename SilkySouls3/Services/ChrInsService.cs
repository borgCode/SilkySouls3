//

using System.Numerics;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Models;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.Services;

public class ChrInsService(IMemoryService memoryService) : IChrInsService
{
    public void SetHp(nint chrIns, int hp) =>
        memoryService.Write(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Hp, hp);

    public int GetCurrentHp(nint chrIns) =>
        memoryService.Read<int>(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Hp);

    public int GetMaxHp(nint chrIns) =>
        memoryService.Read<int>(GetChrData(chrIns) + ChrIns.ChrDataOffsets.MaxHp);

    public void SetMp(nint chrIns, int mp) =>
        memoryService.Write(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Mp, mp);

    public int GetMp(nint chrIns) =>
        memoryService.Read<int>(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Mp);

    public int GetMaxMp(nint chrIns) =>
        memoryService.Read<int>(GetChrData(chrIns) + ChrIns.ChrDataOffsets.MaxMp);

    public void SetSp(nint chrIns, int sp) =>
        memoryService.Write(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Sp, sp);

    public int GetSp(nint chrIns) =>
        memoryService.Read<int>(GetChrData(chrIns) + ChrIns.ChrDataOffsets.Sp);

    public void ToggleNoDamage(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(GetChrData(chrIns) + ChrIns.ChrDataOffsets.NoDamage.Offset,
            ChrIns.ChrDataOffsets.NoDamage.Bit, isEnabled);

    public bool IsNoDamageEnabled(nint chrIns) =>
        memoryService.IsBitSet(GetChrData(chrIns) + ChrIns.ChrDataOffsets.NoDamage.Offset,
            ChrIns.ChrDataOffsets.NoDamage.Bit);

    public float GetSpeed(nint chrIns) =>
        memoryService.Read<float>(GetChrBehavior(chrIns) + ChrIns.ChrBehaviorOffsets.AnimationSpeed);

    public void SetSpeed(nint chrIns, float speed) =>
        memoryService.Write(GetChrBehavior(chrIns) + ChrIns.ChrBehaviorOffsets.AnimationSpeed, speed);

    public void ToggleInfinitePoise(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(GetChrSuperArmor(chrIns) + ChrIns.ChrSuperArmorOffsets.InfinitePoise.Offset,
            ChrIns.ChrSuperArmorOffsets.InfinitePoise.Bit, isEnabled);

    public bool IsInfinitePoiseEnabled(nint chrIns) =>
        memoryService.IsBitSet(GetChrSuperArmor(chrIns) + ChrIns.ChrSuperArmorOffsets.InfinitePoise.Offset,
            ChrIns.ChrSuperArmorOffsets.InfinitePoise.Bit);

    public nint ChrInsByEntityId(int entityId)
    {
        var bytes = AsmLoader.GetAsmBytes(AsmScript.ChrInsByEntityId);
        var storeLocation = CustomCodeOffsets.Base + CustomCodeOffsets.StoredChrInsByEntityId;
        AsmHelper.WriteAbsoluteAddresses(bytes, [
            (Functions.ChrInsByEntityId, 0xF + 2),
            (storeLocation, 0x1b + 2)
        ]);

        AsmHelper.WriteImmediateDword(bytes, entityId, 0x9 + 2);
        memoryService.AllocateAndExecute(bytes);
        return memoryService.Read<nint>(storeLocation);
    }

    public void RequestEventAnimation(nint chrIns, int animationId) =>
        memoryService.Write(GetChrEvent(chrIns) + ChrIns.ChrEventOffsets.AnimationRequest, animationId);

    public int GetCurrentAnimationId(nint chrIns) =>
        memoryService.Read<int>(GetTimeAct(chrIns) + ChrIns.ChrTimeActOffsets.CurrentAnimationId);

    public Poise GetPoise(nint chrIns)
    {
        const int start = ChrIns.ChrSuperArmorOffsets.Poise;
        const int blockSize = ChrIns.ChrSuperArmorOffsets.PoiseTimer - start + sizeof(float);
        var block = new MemoryBlock(memoryService.ReadBytes(GetChrSuperArmor(chrIns) + start, blockSize));
        return new Poise
        {
            Current = block.Get<float>(ChrIns.ChrSuperArmorOffsets.Poise - start),
            Max = block.Get<float>(ChrIns.ChrSuperArmorOffsets.MaxPoise - start),
            Timer = block.Get<float>(ChrIns.ChrSuperArmorOffsets.PoiseTimer - start),
        };
    }

    public Defenses GetDefenses(nint chrIns)
    {
        const int blockSize = sizeof(float) * 8;
        var block = new MemoryBlock(memoryService.ReadBytes(GetNpcParam(chrIns) + ChrIns.NpcParamOffsets.Absorptions,
            blockSize));
        return new Defenses
        {
            Standard = block.Get<float>(0 * sizeof(float)),
            Slash = block.Get<float>(1 * sizeof(float)),
            Strike = block.Get<float>(2 * sizeof(float)),
            Thrust = block.Get<float>(3 * sizeof(float)),
            Magic = block.Get<float>(4 * sizeof(float)),
            Fire = block.Get<float>(5 * sizeof(float)),
            Lightning = block.Get<float>(6 * sizeof(float)),
            Dark = block.Get<float>(7 * sizeof(float)),
        };
    }

    public Vector3 GetPosition(nint chrIns) =>
        memoryService.Read<Vector3>(GetChrPhysics(chrIns) + ChrIns.ChrPhysicsOffsets.Position);

    public void SetPosition(nint chrIns, Vector3 position) =>
        memoryService.Write(GetChrPhysics(chrIns), position);

    public void ToggleFreezeAi(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.DisableAi, isEnabled);

    public bool IsFreezeAiEnabled(nint chrIns) =>
        memoryService.IsBitSet(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.DisableAi);

    public void ToggleNoHit(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoHit, isEnabled);

    public bool IsNoHitEnabled(nint chrIns) =>
        memoryService.IsBitSet(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoHit);

    public void ToggleNoAttack(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoAttack, isEnabled);

    public bool IsNoAttackEnabled(nint chrIns) =>
        memoryService.IsBitSet(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoAttack);

    public void ToggleNoMove(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoMove, isEnabled);

    public bool IsNoMoveEnabled(nint chrIns) =>
        memoryService.IsBitSet(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoMove);

    public void ToggleNoGoodsConsume(nint chrIns, bool isEnabled) =>
        memoryService.SetBitValue(chrIns + ChrIns.Flags, (int)ChrIns.FlagsBits.NoGoodsConsume, isEnabled);

    public Immunities GetImmunities(nint chrIns)
    {
        var npcParam = GetNpcParam(chrIns);
        return new Immunities
        {
            Poison = memoryService.Read<int>(npcParam + ChrIns.NpcParamOffsets.PoisonImmunity) == 30000,
            Toxic = memoryService.Read<int>(npcParam + ChrIns.NpcParamOffsets.ToxicImmunity) == 30010,
            Bleed = memoryService.Read<int>(npcParam + ChrIns.NpcParamOffsets.BleedImmunity) == 30020,
            Frost = memoryService.Read<int>(npcParam + ChrIns.NpcParamOffsets.FrostBiteImmunity) == 30040,
        };
    }

    public Resistances GetResistances(nint chrIns)
    {
        const int start = (int)WorldChrManImp.ChrResistModule.PoisonCurrent;
        const int blockSize = (int)WorldChrManImp.ChrResistModule.FrostMax - start + sizeof(int);
        var block = new MemoryBlock(memoryService.ReadBytes(GetChrResist(chrIns) + start, blockSize));
        return new Resistances
        {
            PoisonCurrent = block.Get<int>((int)WorldChrManImp.ChrResistModule.PoisonCurrent - start),
            ToxicCurrent = block.Get<int>((int)WorldChrManImp.ChrResistModule.ToxicCurrent - start),
            BleedCurrent = block.Get<int>((int)WorldChrManImp.ChrResistModule.BleedCurrent - start),
            FrostCurrent = block.Get<int>((int)WorldChrManImp.ChrResistModule.FrostCurrent - start),
            PoisonMax = block.Get<int>((int)WorldChrManImp.ChrResistModule.PoisonMax - start),
            ToxicMax = block.Get<int>((int)WorldChrManImp.ChrResistModule.ToxicMax - start),
            BleedMax = block.Get<int>((int)WorldChrManImp.ChrResistModule.BleedMax - start),
            FrostMax = block.Get<int>((int)WorldChrManImp.ChrResistModule.FrostMax - start),
        };
    }

    public float GetHitRadius(nint chrIns) =>
        memoryService.Read<float>(GetChrPhysics(chrIns) + ChrIns.ChrPhysicsOffsets.HitRadius);

    public float GetDistBetweenChrs(nint chrIns1, nint chrIns2)
    {
        var chrPhysicsModule1 = GetChrPhysics(chrIns1);
        var chrPhysicsModule2 = GetChrPhysics(chrIns2);
        var chrPos1 = memoryService.Read<Vector3>(chrPhysicsModule1 + ChrIns.ChrPhysicsOffsets.Position);
        var chrPos2 = memoryService.Read<Vector3>(chrPhysicsModule2 + ChrIns.ChrPhysicsOffsets.Position);
        var chrHitRadius1 = memoryService.Read<float>(chrPhysicsModule1 + ChrIns.ChrPhysicsOffsets.HitRadius);
        var chrHitRadius2 = memoryService.Read<float>(chrPhysicsModule2 + ChrIns.ChrPhysicsOffsets.HitRadius);
        return Vector3.Distance(chrPos1, chrPos2) - chrHitRadius1 - chrHitRadius2;
    }

    public void ForceSetPosition(nint chrIns, Vector4 position)
    {
        var physicsModule = GetChrPhysics(chrIns);
        memoryService.Write(physicsModule + ChrIns.ChrPhysicsOffsets.Position, position);
        memoryService.Write(physicsModule + ChrIns.ChrPhysicsOffsets.PrevPosition, position);
        memoryService.Write(physicsModule + ChrIns.ChrPhysicsOffsets.PhysicsDirty, true);
    }

    #region Private Methods

    private nint GetChrData(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrDataModule, true);

    private nint GetChrResist(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrResistModule, true);

    private nint GetChrBehavior(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrBehaviorModule, true);

    private nint GetChrSuperArmor(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrSuperArmorModule, true);

    private nint GetChrEvent(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrEventModule, true);

    private nint GetChrPhysics(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrPhysicsModule, true);

    private nint GetNpcParam(nint chrIns) =>
        memoryService.FollowPointers(chrIns, [..ChrIns.AiThink, ChrIns.AiThinkOffsets.NpcParam], true);

    private nint GetTimeAct(nint chrIns) =>
        memoryService.FollowPointers(chrIns, ChrIns.ChrTimeActModule, true);

    #endregion
}