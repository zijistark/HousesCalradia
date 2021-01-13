using System;
using System.Linq;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace HousesCalradia.Patches
{
    [HarmonyPatch(typeof(KillCharacterAction))]
    internal sealed class KillCharacterActionPatch
    {
        internal static class ForOptimizer
        {
            internal static void DoApplyInternalPrefix() => ApplyInternalPrefix(null!, null!, KillCharacterAction.KillCharacterActionDetail.None, true);
        }

        private static class Helpers
        {
            internal static bool IsClanExtinctionPreventionDisallowed(Clan? clan, Hero? victim)
                => clan is null
                || clan.Leader is null
                || clan.Leader != victim
                || clan.Kingdom is null
                || clan.Kingdom.IsEliminated
                || clan.IsRebelClan
                || clan.IsBanditFaction
                || clan == CampaignData.NeutralFaction
                || victim == Hero.MainHero
                || clan.Lords.Where(h => h.IsAlive && !h.IsChild && h.IsActive && h.IsNoble && h != victim).Any();
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch("ApplyInternal")]
        static void ApplyInternalPrefix(
            Hero? victim,
            Hero? killer,
            KillCharacterAction.KillCharacterActionDetail actionDetail,
            bool showNotification)
        {
            _ = showNotification;

            if (victim is null)
                return;

            var clan = victim.Clan;

            // Only interested in the death of regular clan leaders where there's no other adult noble to succeed them:
            if (Helpers.IsClanExtinctionPreventionDisallowed(clan, victim))
                return;

            if (Config.AllowPlayerExecutionToEliminateClan &&
                killer == Hero.MainHero &&
                actionDetail == KillCharacterAction.KillCharacterActionDetail.Executed)
            {
                return;
            }

            Util.Log.Print($"[{CampaignTime.Now}] CLAN EXTINCTION PREVENTION: Leader of clan {clan.Name}, " +
                $"{victim.Name} of age {victim.Age:F0}, died without a valid heir (reason: " +
                $"{Enum.GetName(typeof(KillCharacterAction.KillCharacterActionDetail), actionDetail)})!");

            // Spawn a male noble "distant relative" into the clan
            var ageMin = Math.Max(22, Campaign.Current.Models.AgeModel.HeroComesOfAge + 1);
            var successor = HeroUtil.SpawnNoble(clan, ageMin, ageMax: ageMin + 10, isFemale: false);

            if (successor is null)
                Util.Log.Print(" -> ERROR: Could not find a noble character template to spawn lord!");
            else
                Util.Log.Print($" -> Summoned distant relative {successor.Name} of age {successor.Age:F0} " +
                    $"to assume leadership of clan {successor.Clan.Name}.");
        }
    }
}
