using System.Collections.Generic;

namespace HousesCalradia
{
    internal static class Config
    {
        internal static bool AllowSameKingdomDiffCultureMarriage { get; set; } = true;
        internal static bool AllowDiffKingdomSameCultureMarriage { get; set; } = true;
        internal static bool AllowDiffKingdomDiffCultureMarriage { get; set; } = false;
        internal static bool AllowDiffKingdomMarriageForRulingClans { get; set; } = true;
        internal static bool SpawnNobleWives { get; set; } = true;
        internal static int MinMaleMarriageAge { get; set; } = 27;
        internal static int MinFemaleMarriageAge { get; set; } = 27;
        internal static int MaxFemaleMarriageAge { get; set; } = 41;
        internal static float MarriageChanceMult { get; set; } = 1f;
        internal static float SpawnedMarriageChanceMult { get; set; } = 1f;
        internal static bool AllowPlayerExecutionToEliminateClan { get; set; } = true;

        internal static void CopyFromSettings(Settings settings)
        {
            AllowSameKingdomDiffCultureMarriage = settings.AllowSameKingdomDiffCultureMarriage;
            AllowDiffKingdomSameCultureMarriage = settings.AllowDiffKingdomSameCultureMarriage;
            AllowDiffKingdomDiffCultureMarriage = settings.AllowDiffKingdomDiffCultureMarriage;
            AllowDiffKingdomMarriageForRulingClans = settings.AllowDiffKingdomMarriageForRulingClans;
            SpawnNobleWives = settings.SpawnNobleWives;
            MinMaleMarriageAge = settings.MinMaleMarriageAge;
            MinFemaleMarriageAge = settings.MinFemaleMarriageAge;
            MaxFemaleMarriageAge = settings.MaxFemaleMarriageAge;
            MarriageChanceMult = settings.MarriageChanceMult;
            SpawnedMarriageChanceMult = settings.SpawnedMarriageChanceMult;
            AllowPlayerExecutionToEliminateClan = settings.AllowPlayerExecutionToEliminateClan;
        }

        internal static List<string> ToStringLines(uint indentSize = 0)
        {
            string prefix = string.Empty;

            for (uint i = 0; i < indentSize; ++i)
                prefix += " ";

            return new List<string>
            {
                $"{prefix}{nameof(AllowSameKingdomDiffCultureMarriage)}    = {AllowSameKingdomDiffCultureMarriage}",
                $"{prefix}{nameof(AllowDiffKingdomSameCultureMarriage)}    = {AllowDiffKingdomSameCultureMarriage}",
                $"{prefix}{nameof(AllowDiffKingdomDiffCultureMarriage)}    = {AllowDiffKingdomDiffCultureMarriage}",
                $"{prefix}{nameof(AllowDiffKingdomMarriageForRulingClans)} = {AllowDiffKingdomMarriageForRulingClans}",
                $"{prefix}{nameof(SpawnNobleWives)}                        = {SpawnNobleWives}",
                $"{prefix}{nameof(MinMaleMarriageAge)}                     = {MinMaleMarriageAge}",
                $"{prefix}{nameof(MinFemaleMarriageAge)}                   = {MinFemaleMarriageAge}",
                $"{prefix}{nameof(MaxFemaleMarriageAge)}                   = {MaxFemaleMarriageAge}",
                $"{prefix}{nameof(MarriageChanceMult)}                     = {MarriageChanceMult}",
                $"{prefix}{nameof(SpawnedMarriageChanceMult)}              = {SpawnedMarriageChanceMult}",
                $"{prefix}{nameof(AllowPlayerExecutionToEliminateClan)}    = {AllowPlayerExecutionToEliminateClan}",
            };
        }
    }
}
