using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace HousesCalradia.Patches
{
	[HarmonyPatch(typeof(KillCharacterAction))]
	class KillCharacterActionPatch
	{
		[HarmonyPrefix]
		[HarmonyPriority(Priority.HigherThanNormal)]
		[HarmonyPatch("ApplyInternal")]
		static void ApplyInternalPrefix(
			Hero victim,
			Hero killer,
			KillCharacterAction.KillCharacterActionDetail actionDetail,
			bool showNotification)
		{
			_ = (killer, showNotification);

			// Only interested in the death of clan leaders where there's no other adult noble to succeed them:
			if (victim == null ||
				victim.Clan == null ||
				victim.Clan.Leader != victim ||
				victim == Hero.MainHero || // No clan extinction prevention for the player (though this might be a nice option)
				victim.Clan.Lords.Where(h => h.IsAlive && !h.IsChild && h.IsActive && h.IsNoble && h != victim).Any())
				return;

			var deathReasonStr = Enum.GetName(typeof(KillCharacterAction.KillCharacterActionDetail), actionDetail);

			Util.Log.Print($"[{CampaignTime.Now}] CLAN EXTINCTION PREVENTION: Leader of clan {victim.Clan.Name}, " +
				$"{victim.Name} of age {victim.Age:F0}, died without a valid heir (reason: {deathReasonStr})!");

			// Spawn a male noble "distant relative" into the clan
			var ageMin = Campaign.Current.Models.AgeModel.HeroComesOfAge + 1;
			var successor = HeroUtil.SpawnNoble(victim.Clan, ageMin, ageMax: ageMin + 10);

			if (successor == null)
				Util.Log.Print(" -> ERROR: Could not find a noble character template to spawn lord!");
			else
				Util.Log.Print($" -> Summoned distant relative {successor.Name} of age {successor.Age:F0} " +
					$"to assume leadership of clan {successor.Clan.Name}.");
		}
	}
}
