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
        internal static class ReferenceStuff
        {
            internal static void DontReally() => ApplyInternalPrefix(null!, null!, KillCharacterAction.KillCharacterActionDetail.None, true);
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch("ApplyInternal")]
        static void ApplyInternalPrefix(
            Hero victim,
            Hero killer,
            KillCharacterAction.KillCharacterActionDetail actionDetail,
            bool showNotification)
        {
            _ = showNotification;

            if (victim is null || victim == Hero.MainHero)
                return;

            var clan = victim.Clan;

            // Only interested in the death of regular clan leaders where there's no other adult noble to succeed them:
            if (clan is null ||
                clan.Leader != victim ||
                clan.Kingdom is null ||
                clan.Kingdom.IsEliminated ||
                clan.IsClanTypeMercenary ||
                // Start extreme paranoia:
                clan.IsRebelFaction ||
                clan.IsBanditFaction ||
                // End extreme paranoia!
                clan.Lords.Where(h => h.IsAlive && !h.IsChild && h.IsActive && h.IsNoble && h != victim).Any())
            {
                return;
            }

            if (Config.AllowPlayerExecutionToEliminateClan &&
                killer == Hero.MainHero &&
                actionDetail == KillCharacterAction.KillCharacterActionDetail.Executed)
            {
                return;
            }

            var deathReasonStr = Enum.GetName(typeof(KillCharacterAction.KillCharacterActionDetail), actionDetail);

            Util.Log.Print($"[{CampaignTime.Now}] CLAN EXTINCTION PREVENTION: Leader of clan {clan.Name}, " +
                $"{victim.Name} of age {victim.Age:F0}, died without a valid heir (reason: {deathReasonStr})!");

            // Spawn a male noble "distant relative" into the clan
            var ageMin = Campaign.Current.Models.AgeModel.HeroComesOfAge + 1;
            var successor = HeroUtil.SpawnNoble(clan, ageMin, ageMax: ageMin + 10, isFemale: MBRandom.RandomFloat < 0.5);

            if (successor is null)
                Util.Log.Print(" -> ERROR: Could not find a noble character template to spawn lord!");
            else
                Util.Log.Print($" -> Summoned distant relative {successor.Name} of age {successor.Age:F0} " +
                    $"to assume leadership of clan {successor.Clan.Name}.");
        }
    }
}
