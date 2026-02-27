// 

namespace SilkySouls3.GameIds;

public static class EzState
{
    public class TalkCommand(int commandId, int[] @params)
    {
        public int CommandId { get; } = commandId;
        public int[] Params { get; } = @params;
    }

    public static class TalkCommands
    {
        public static TalkCommand OpenRegularShop(int start, int end) => new(22, [start, end]);
        public static readonly TalkCommand Repair = new(23, []);
        public static readonly TalkCommand OpenUpgrade = new(24, [0]);
        public static readonly TalkCommand OpenAttunement = new(28, [1000, 1000]);
        public static readonly TalkCommand LevelUp = new(31, []);
        public static readonly TalkCommand WarpMenu = new(41, [-1]);
        public static readonly TalkCommand OpenInfuse = new(48, []);
        public static readonly TalkCommand OpenAllotEstus = new(105, []);

        public static readonly TalkCommand[] UpgradeMenuFlags =
        [
            new(49, [6001, 232]),
            new(49, [6001, 233]),
            new(49, [6001, 234]),
        ];

        public static readonly TalkCommand[] InfuseMenuFlags =
        [
            new(49, [6001, 344]),
            new(49, [6001, 337]),
            new(49, [6001, 334]),
            new(49, [300, 332]),
            new(49, [300, 333]),
            new(49, [300, 342]),
            new(49, [301, 335]),
            new(49, [301, 345]),
            new(49, [301, 340]),
            new(49, [302, 336]),
            new(49, [302, 338]),
            new(49, [302, 339]),
            new(49, [303, 341]),
            new(49, [303, 343]),
            new(49, [303, 346]),
            new(49, [6000, 347]),
            new(49, [6001, 331])
        ];
    }
}