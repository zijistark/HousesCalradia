using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace HousesCalradia
{
	class MarriageBehavior : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(OnDailyHeroTick));
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
		}

		public override void SyncData(IDataStore dataStore) {}

		protected void OnSessionLaunched(CampaignGameStarter starter) => SetParameters();

		protected void OnDailyHeroTick(Hero hero)
		{
			// Very early exit conditions:
			if (hero.IsFemale || !hero.IsNoble)
				return;

			// We only evaluate marriage once per human year, and we use an offset to distribute hero
			// marriages more evenly throughout that year:
			int daysOffset = hero.Id.GetHashCode() % daysPerHumanYear;
			int daysElapsed = (int)Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow;

			// Is this not the right day to do our yearly marriage tick?
			if ((daysElapsed + daysOffset) % daysPerHumanYear != 0)
				return;

			// Does this hero even qualify for a marriage evaluation?
			if (hero.IsDead ||
				!hero.IsActive ||
				hero.Clan == null ||
				hero.Clan.Kingdom == null ||
				(int)hero.Age < minAgeMale ||
				!Campaign.Current.Models.MarriageModel.IsSuitableForMarriage(hero) ||
				hero.Clan.IsClanTypeMercenary ||
				hero.Clan == Clan.PlayerClan)
				return;

			var clanFitness = GetClanFitness(hero.Clan);
			var marriageChance = GetAnnualMarriageChance(clanFitness);

			Util.Log.Print($"[{CampaignTime.Now}] {GetHeroTrace(hero, clanFitness)}: Considering marriage ({marriageChance * 100:F1}% chance)...");

			if (MBRandom.RandomFloat > marriageChance)
			{
				Util.Log.Print(" -> Decided not to marry for now.");
				return;
			}

			// Find eligible candidates for marriage in order of preference
			var wife = Kingdom.All
				.Where(k => !k.IsEliminated)
				.SelectMany(k => k.Clans)
				.Where(c => !c.IsClanTypeMercenary && c != Clan.PlayerClan)
				.SelectMany(c => c.Lords)
				.Where(h =>
					h.IsFemale &&
					h.IsAlive &&
					h.IsNoble &&
					h.IsActive &&
					h.Spouse == null &&
					(int)h.Age >= minAgeFemale &&
					(int)h.Age <= maxAgeFemale &&
					Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(hero, h))
				.OrderByDescending(h => GetNobleMatchScore(hero, h))
				.FirstOrDefault();

			var marriageType = string.Empty;

			// Were there no eligible female nobles?
			if (wife == null)
			{
				string spawnMsg = " -> No eligible candidates to marry.";

				// If CF >= 3, then we never spawn a wife.
				if (clanFitness >= 3)
				{
					Util.Log.Print(spawnMsg + " Can't spawn wife, because our clan fitness is too high.");
					return;
				}

				// Likewise, at 0 < CF < 3, there are restrictions upon spawning a wife based
				// upon how many preexisting children the hero has sired and/or whether they're
				// too old.
				int childCount = hero.Children.Count();
				int maleChildCount = hero.Children.Where(h => !h.IsFemale).Count();

				if ((clanFitness == 2 && (childCount >= 3 || maleChildCount >= 1 || hero.Age >= 60)) ||
					(clanFitness == 1 && (childCount >= 4 || maleChildCount >= 2 || hero.Age >= 70)))
				{
					Util.Log.Print(spawnMsg + " Can't spawn wife, because our clan fitness is too high for our prior children or age.");
					return;
				}

				// Now, the base chance from here (taking into account that our clan fitness level
				// has already significantly affected the odds of reaching this point) is simply
				// 40%, with up to two +5% bonuses or a -5% malues for however many children short
				// of 2 we do not already have (i.e., in [35%, 50%]).

				float spawnChance = 0.4f + Math.Max(-0.05f, 0.05f * (2 - childCount));
				string chanceStr = $" (chance was {spawnChance * 100:F0}%)";

				if (MBRandom.RandomFloat > spawnChance)
				{
					Util.Log.Print(spawnMsg + $" Decided not to spawn wife{chanceStr}.");
					return;
				}

				Util.Log.Print(spawnMsg + $" Spawning wife{chanceStr}...");
				marriageType = " (spawned)";

				var originClan = Kingdom.All
					.SelectMany(k => k.Clans)
					.Where(c =>
						c.Culture == hero.Culture &&
						!c.IsEliminated &&
						c.Leader != null &&
						c.Leader.IsAlive &&
						c != Clan.PlayerClan &&
						c != hero.Clan)
					.GetRandomElement() ?? hero.Clan;

				int wifeAgeMin = Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale;
				int wifeAgeMax = Math.Min(maxAgeFemale - 5, wifeAgeMin + 5);

				wife = HeroUtil.SpawnNoble(originClan, wifeAgeMin, wifeAgeMax, isFemale: true);
				wife.IsFertile = true;

				if (wife == null)
				{
					Util.Log.Print(" ---> ERROR: Could not find character template to spawn female noble!");
					return;
				}
			}

			// Get married!
			Util.Log.Print($" -> MARRIAGE{marriageType}: {GetHeroTrace(wife, GetClanFitness(wife.Clan))}");
			MarriageAction.Apply(hero, wife);
		}

		protected int GetClanFitness(Clan clan) => clan.Lords
			.Where(h =>
				!h.IsFemale &&
				h.IsAlive &&
				h.IsActive &&
				h.Spouse != null &&
				(int)h.Spouse.Age < maxFemaleReproductionAge)
			.Count();

		protected float GetAnnualMarriageChance(int clanFitness) => (float)Math.Pow(2, -clanFitness);

		protected string GetHeroTrace(Hero h, int clanFitness = -1)
		{
			string fitnessStr = (clanFitness < 0) ? string.Empty : $" (CF={clanFitness})";
			return $"{h.Name} {h.Clan.Name}{fitnessStr} of {h.Clan.Kingdom.Name} (age {h.Age:F0})";
		}

		protected float GetNobleMatchScore(Hero suitor, Hero maiden) =>
			(maiden.Clan.Kingdom == suitor.Clan.Kingdom ? 8000 : 0) + (maiden.Culture == suitor.Culture ? 4000 : 0) - maiden.Age;

		protected void SetParameters()
		{
			minAgeMale = Math.Max(minAgeMaleDefault, Campaign.Current.Models.MarriageModel.MinimumMarriageAgeMale);
			minAgeFemale = Math.Max(minAgeFemaleDefault, Campaign.Current.Models.MarriageModel.MinimumMarriageAgeFemale);
			daysPerHumanYear = GetDaysPerHumanYear();

			var trace = new List<string>
			{
				$"Min. Age to Marry (Male):   {minAgeMale}",
				$"Min. Age to Marry (Female): {minAgeFemale}",
				$"Days Per Human-Year:        {daysPerHumanYear}"
			};

			Util.Log.Print(trace);
		}

		protected int GetDaysPerHumanYear()
		{
			int ret = daysPerHumanYearDefault;

			// First, see if Pacemaker is also loaded, and if so, get a handle to it.
			var pacemakerAsm = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == "Pacemaker");

			if (pacemakerAsm == null)
				return ret;

			// Let's dig into the Pacemaker types, namely Pacemaker.Export
			var api = pacemakerAsm.ExportedTypes.SingleOrDefault(t => t.FullName == "Pacemaker.Export");

			if (api == null)
				return ret;

			var apiMethod = api.GetMethod("GetDaysPerHumanYear");

			if (apiMethod != null)
				ret = (int)Math.Round((float)apiMethod.Invoke(null, new object[] { true }));

			if (ret <= 0)
				ret = 1;

			return ret;
		}

		private int minAgeMale;
		private int minAgeFemale;
		private int daysPerHumanYear;

		private const int maxFemaleReproductionAge = 45;
		private const int maxAgeFemale = maxFemaleReproductionAge - 5;
		private const int minAgeMaleDefault = 27;
		private const int minAgeFemaleDefault = minAgeMaleDefault;
		private const int daysPerHumanYearDefault = 21 * 4; // Vanilla timescale of 21 days/season
	}
}
