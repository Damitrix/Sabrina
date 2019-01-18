using System;

public static class UserSettingsExtension
{
    public enum DungeonDifficulty
    {
        Nonexistant,
        Easy,
        Beginner,
        Normal,
        NormalPlus,
        Hard,
        Harder,
        Extreme
    }

    public enum DenialReason
    {
        None,
        Cooldown
    }

    [Flags]
    public enum LockReason
    {
        None = 1,
        Cooldown = 2,
        Task = 4,
        Extension = 8
    }
}