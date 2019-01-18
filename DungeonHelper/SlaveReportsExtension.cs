using System;

public static class WheelExtension
{
    [Flags]
    public enum Outcome
    {
        NotSet = 1,
        Denial = 2,
        Ruin = 4,
        Orgasm = 8,
        Edge = 16,
        Task = 32
    }

    public enum WheelDifficultyPreference
    {
        Baby,
        Easy,
        Default,
        Hard,
        Masterbater
    }

    [Flags]
    public enum WheelTaskPreferenceSetting
    {
        Default,
        Task,
        Time,
        Amount
    }
}