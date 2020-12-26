using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;

namespace HousesCalradia
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => $"{SubModule.Name}_v1";
        public override string DisplayName => SubModule.DisplayName;
        public override string FolderName => SubModule.Name;
        public override string FormatType => "json2";

        private const string AllowSameKingdomDiffCultureMarriage_Hint = "Allow marriages within the same kingdom with a " +
            "noble of a different culture. Note that same-kingdom, same-culture marriages are always allowed, and " +
            "same-culture pairings are always preferred. [ Default: ON ]";

        private const string AllowDiffKingdomSameCultureMarriage_Hint = "Allow marriages between different kingdoms if " +
            "the couple shares the same culture. Same-kingdom pairings will still always be preferred. Excludes ruling " +
            "clans unless that setting is enabled. [ Default: ON ]";

        private const string AllowDiffKingdomDiffCultureMarriage_Hint = "Allow marriages between different kingdoms " +
            "even if the couple doesn't share the same culture. Same-kingdom and/or same-culture pairings will still " +
            "always be preferred. Excludes ruling clans unless that setting is enabled. [ Default: OFF ]";

        private const string AllowDiffKingdomMarriageForRulingClans_Hint = "Allow kingdom rulers' clans to marry " +
            "into families in different kingdoms in whichever cases different-kingdom marriage is allowed (if any). " +
            "[ Default: ON ]";

        private const string SpawnNobleWives_Hint = "If there are no eligible noble candidates and their clan " +
            "desperately needs a marriage to survive, allow nobles a chance to marry a spawned spouse of " +
            "their culture and kingdom. Strongly recommended. [ Default: ON ]";

        private const string MinMaleMarriageAge_Hint = "Below this age, the matchmaking system will not consider men " +
            "for marriage. If you set this too low, there will be far fewer potential spouses for the player clan. " +
            "[ Default: 27 ]";

        private const string MinFemaleMarriageAge_Hint = "Below this age, the matchmaking system will not consider women " +
            "for marriage. If you set this too low, there will be far fewer potential spouses for the player clan. " +
            "[ Default: 27 ]";

        private const string MaxFemaleMarriageAge_Hint = "Above this age, the matchmaking system will not consider women " +
            "for marriage. If you set this too close to menopause (45), more marriages will bear no children. " +
            "[ Default: 41 ]";

        private const string MarriageChanceMult_Hint = "Multiplied with the annual marriage consideration chance of a " +
            "noble. The base chance halves at each tier of their clan's fitness to survive. Recommended to stay below " +
            "150% / 1.5x for proper clan fitness prioritization. [ Default: 100% ]";

        private const string SpawnedMarriageChanceMult_Hint = "If a clan desperate for a marriage while there are no " +
            "eligible candidates qualifies for a chance to marry one of the lesser nobility, this is multiplied with " +
            "that chance. Base chance varies with many factors. [ Default: 100% ]";

        [SettingPropertyBool("Allow Same-Kingdom, Different-Culture Marriage", HintText = AllowSameKingdomDiffCultureMarriage_Hint, RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("AI Noble Marriage", GroupOrder = 0)]
        public bool AllowSameKingdomDiffCultureMarriage { get; set; } = Config.AllowSameKingdomDiffCultureMarriage;

        [SettingPropertyBool("Allow Different-Kingdom, Same-Culture Marriage", HintText = AllowDiffKingdomSameCultureMarriage_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public bool AllowDiffKingdomSameCultureMarriage { get; set; } = Config.AllowDiffKingdomSameCultureMarriage;

        [SettingPropertyBool("Allow Different-Kingdom, Different-Culture Marriage", HintText = AllowDiffKingdomDiffCultureMarriage_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public bool AllowDiffKingdomDiffCultureMarriage { get; set; } = Config.AllowDiffKingdomDiffCultureMarriage;

        [SettingPropertyBool("Allow Different-Kingdom Marriages for Ruling Clans", HintText = AllowDiffKingdomMarriageForRulingClans_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public bool AllowDiffKingdomMarriageForRulingClans { get; set; } = Config.AllowDiffKingdomMarriageForRulingClans;

        // public bool EnableMinorFactionMarriage { get; set; } = true;

        [SettingPropertyBool("Allow Marriage of Lesser Nobility", HintText = SpawnNobleWives_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public bool SpawnNobleWives { get; set; } = Config.SpawnNobleWives;

        [SettingPropertyInteger("Male Minimum Age to Marry", 18, 35, HintText = MinMaleMarriageAge_Hint, RequireRestart = false, Order = 4)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public int MinMaleMarriageAge { get; set; } = Config.MinMaleMarriageAge;

        [SettingPropertyInteger("Female Minimum Age to Marry", 18, 35, HintText = MinFemaleMarriageAge_Hint, RequireRestart = false, Order = 5)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public int MinFemaleMarriageAge { get; set; } = Config.MinFemaleMarriageAge;

        [SettingPropertyInteger("Female Maximum Age to Marry", 36, 44, HintText = MaxFemaleMarriageAge_Hint, RequireRestart = false, Order = 6)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public int MaxFemaleMarriageAge { get; set; } = Config.MaxFemaleMarriageAge;

        [SettingPropertyFloatingInteger("Marriage Consideration Chance Multiplier", 0f, 2f, "#0%", HintText = MarriageChanceMult_Hint, RequireRestart = false, Order = 7)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public float MarriageChanceMult { get; set; } = Config.MarriageChanceMult;

        [SettingPropertyFloatingInteger("Lesser Nobility Marriage Chance Multiplier", 0f, 2f, "#0%", HintText = SpawnedMarriageChanceMult_Hint, RequireRestart = false, Order = 8)]
        [SettingPropertyGroup("AI Noble Marriage")]
        public float SpawnedMarriageChanceMult { get; set; } = Config.SpawnedMarriageChanceMult;

        ///////

        private const string AllowPlayerExecutionToEliminateClan_Hint = "If the final surviving adult noble in a " +
            "clan (i.e., the leader) is executed by the player, then the clan will be allowed to go extinct.";

        [SettingPropertyBool("Execution Can Eliminate Noble Clans", HintText = AllowPlayerExecutionToEliminateClan_Hint, RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("Clan Extinction Prevention", GroupOrder = 1)]
        public bool AllowPlayerExecutionToEliminateClan { get; set; } = Config.AllowPlayerExecutionToEliminateClan;
    }
}
