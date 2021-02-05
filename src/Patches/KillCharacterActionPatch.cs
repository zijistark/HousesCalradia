using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace HousesCalradia.Patches
{
    internal sealed class KillCharacterActionPatch : Patch
    {
        private static readonly Reflect.Method TargetMethod = new(typeof(KillCharacterAction), "ApplyInternal");
        private static readonly Reflect.Method<KillCharacterActionPatch> PatchMethod = new(nameof(ApplyInternalPrefix));

        internal KillCharacterActionPatch() : base(Type.Prefix, TargetMethod, PatchMethod, HarmonyLib.Priority.VeryHigh) { }

        private static bool IsClanExtinctionPreventionDisallowed(Clan? clan, Hero victim)
            => clan is null
            || clan.Leader is null
            || clan.Leader != victim
            || clan.Kingdom is null
            || clan.Kingdom.IsEliminated
            || clan.IsRebelClan
            || clan.IsBanditFaction
            || clan == CampaignData.NeutralFaction
            || victim == Hero.MainHero
            || clan.Lords.Any(h => h.IsAlive && !h.IsChild && h.IsActive && h.IsNoble && h != victim);

        private static void ApplyInternalPrefix(Hero? victim,
                                                Hero? killer,
                                                KillCharacterAction.KillCharacterActionDetail actionDetail)
        {
            // Only interested in the death of regular clan leaders where there's no other adult noble to succeed them:
            if (victim is null || IsClanExtinctionPreventionDisallowed(victim.Clan, victim))
                return;

            // If configured, allow player executions to eliminate clans:
            if (Config.AllowPlayerExecutionToEliminateClan
                && killer == Hero.MainHero
                && actionDetail == KillCharacterAction.KillCharacterActionDetail.Executed)
            {
                return;
            }

            Util.Log.Print($"[{CampaignTime.Now}] CLAN EXTINCTION PREVENTION: Leader of clan {victim.Clan.Name},"
                         + $" {victim.Name} of age {victim.Age:F0}, died without a valid heir (reason:"
                         + $" {Enum.GetName(typeof(KillCharacterAction.KillCharacterActionDetail), actionDetail)})!");

            // Spawn a male noble "distant relative" into the clan
            var ageMin = Math.Max(22, Campaign.Current.Models.AgeModel.HeroComesOfAge + 1);
            var successor = HeroUtil.SpawnNoble(victim.Clan, ageMin, ageMax: ageMin + 10, isFemale: false);

            if (successor is null)
                Util.Log.Print(" -> ERROR: Could not find a noble character template to spawn lord!");
            else
                Util.Log.Print($" -> Summoned distant relative {successor.Name} of age {successor.Age:F0}"
                             + $" to assume leadership of clan {successor.Clan.Name}.");
        }
    }
}
