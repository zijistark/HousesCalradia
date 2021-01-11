using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace HousesCalradia
{
    internal sealed class MarriageBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(OnDailyHeroTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore) {}

        private void OnSessionLaunched(CampaignGameStarter starter) => SetParameters();

        private void OnDailyHeroTick(Hero hero)
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
                hero.Clan is null ||
                hero.Clan.Kingdom is null ||
                (int)hero.Age < minAgeMale ||
                !marriageModel!.IsSuitableForMarriage(hero) ||
                hero.Clan.IsClanTypeMercenary ||
                hero.Clan == Clan.PlayerClan ||
                hero.Clan == CampaignData.NeutralFaction)
            {
                return;
            }

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
                .Where(k => !k.IsEliminated
                         && IsKingdomAllowedForMarriageByConfig(hero, k))
                .SelectMany(k => k.Clans)
                .Where(c => !c.IsEliminated
                         && !c.IsClanTypeMercenary
                         && c != CampaignData.NeutralFaction
                         && c != Clan.PlayerClan)
                .SelectMany(c => c.Lords)
                .Where(h => h.IsFemale
                         && h.IsAlive
                         && h.IsNoble
                         && h.IsActive
                         && h.Spouse is null
                         && IsMaidenAllowedForMarriageByConfig(hero, h)
                         && marriageModel!.IsCoupleSuitableForMarriage(hero, h))
                .OrderByDescending(h => GetNobleMatchScore(hero, h))
                .FirstOrDefault();

            var marriageType = string.Empty;

            // Were there no eligible female nobles?
            if (wife is null)
            {
                string spawnMsg = " -> No eligible noble candidates to marry.";

                if (!Config.SpawnNobleWives || Config.SpawnedMarriageChanceMult < 0.01f)
                {
                    Util.Log.Print(spawnMsg);
                    return;
                }

                // If CF >= 3, then we never spawn a wife.
                if (clanFitness >= 3)
                {
                    Util.Log.Print(spawnMsg + " Can't try to spawn wife (clan fitness is too high).");
                    return;
                }

                // Likewise, at 0 < CF < 3, there are restrictions upon spawning a wife based
                // upon how many preexisting children the hero has sired and/or whether they're
                // too old.
                int childCount = hero.Children.Count();
                int maleChildCount = hero.Children.Where(h => !h.IsFemale).Count();

                if (clanFitness == 2 && (childCount >= 2 || maleChildCount >= 1 || hero.Age >= 60) ||
                    clanFitness == 1 && (childCount >= 3 || maleChildCount >= 2 || hero.Age >= 65))
                {
                    Util.Log.Print(spawnMsg + " Can't try to spawn wife (clan fitness is too high for our prior children / age).");
                    return;
                }

                // Now, the base chance from here (taking into account that our clan fitness level
                // has already significantly affected the odds of reaching this point) is simply
                // 40%, with up to two +5% bonuses or two -5% maluses for however many children short
                // of 2 we do not already have (i.e., in [30%, 50%]).

                float spawnChance = 0.4f + Math.Max(-0.1f, 0.05f * (2 - childCount));
                spawnChance *= Config.SpawnedMarriageChanceMult; // Modified by our config
                var chanceStr = $" (chance was {spawnChance * 100:F0}%)";

                if (MBRandom.RandomFloat > spawnChance)
                {
                    Util.Log.Print(spawnMsg + $" Decided not to spawn wife{chanceStr}.");
                    return;
                }

                Util.Log.Print(spawnMsg + $" Spawning wife{chanceStr}...");
                marriageType = " (spawned)";

                int wifeAgeMin = marriageModel!.MinimumMarriageAgeFemale;
                int wifeAgeMax = Math.Min(maxAgeFemale - 5, wifeAgeMin + 5);

                wife = HeroUtil.SpawnNoble(hero.Clan, wifeAgeMin, wifeAgeMax, isFemale: true);

                if (wife is null)
                {
                    Util.Log.Print(" ---> ERROR: Could not find character template to spawn female noble!");
                    return;
                }

                wife.IsFertile = true;
            }

            // Get married!
            Util.Log.Print($" -> MARRIAGE{marriageType}: {GetHeroTrace(wife, GetClanFitness(wife.Clan))}");
            MarriageAction.Apply(hero, wife);
        }

        private int GetClanFitness(Clan clan) => clan.Lords
            .Where(h => !h.IsFemale
                     && h.IsAlive
                     && h.IsActive
                     && h.Spouse is not null
                     && (int)h.Spouse.Age <= maxFemaleReproductionAge)
            .Count();

        private float GetAnnualMarriageChance(int clanFitness) =>
            (float)Math.Pow(2, -clanFitness) * Config.MarriageChanceMult;

        private bool IsKingdomAllowedForMarriageByConfig(Hero suitor, Kingdom kingdom)
        {
            if (kingdom == suitor.Clan.Kingdom)
                return true; // Same-kingdom is always allowed

            // From here on, it must be a different kingdom...

            if (sameKingdomOnly)
                return false;

            if (!Config.AllowDiffKingdomMarriageForRulingClans && suitor.Clan == suitor.Clan.Kingdom.RulingClan)
                return false;

            return !kingdom.IsAtWarWith(suitor.Clan.Kingdom);
        }

        private bool IsMaidenAllowedForMarriageByConfig(Hero suitor, Hero maiden)
        {
            int age = (int)maiden.Age;

            if (age < minAgeFemale || age > maxAgeFemale)
                return false;

            // Address issues with female clan leaders marrying (short of overriding the entire MarriageModel):
            if (maiden == maiden.Clan.Leader)
                return false;

            bool sameKingdom = suitor.Clan.Kingdom == maiden.Clan.Kingdom;

            if (!sameKingdom
                && !Config.AllowDiffKingdomMarriageForRulingClans
                && maiden.Clan == maiden.Clan.Kingdom.RulingClan)
                return false;

            bool sameCulture = suitor.Culture == maiden.Culture;

            return sameKingdom && sameCulture ||
                Config.AllowSameKingdomDiffCultureMarriage && sameKingdom ||
                Config.AllowDiffKingdomSameCultureMarriage && sameCulture ||
                Config.AllowDiffKingdomDiffCultureMarriage && !sameKingdom && !sameCulture;
        }

        private float GetNobleMatchScore(Hero suitor, Hero maiden) =>
              (maiden.Clan.Kingdom == suitor.Clan.Kingdom ? 8000 : 0)
            + (maiden.Culture == suitor.Culture ? 4000 : 0)
            + diplomacyModel!.GetEffectiveRelation(maiden, suitor) / 10f // [-10;10]
            - maiden.Age;

        private string GetHeroTrace(Hero h, int clanFitness = -1)
        {
            string fitnessStr = (clanFitness < 0) ? string.Empty : $" (CF={clanFitness})";
            return $"{h.Name} {h.Clan.Name}{fitnessStr} of {h.Clan.Kingdom.Name} (age {h.Age:F0})";
        }

        private void SetParameters()
        {
            diplomacyModel = Campaign.Current.Models.DiplomacyModel;
            marriageModel = Campaign.Current.Models.MarriageModel;
            minAgeMale = Math.Max(Config.MinMaleMarriageAge, marriageModel.MinimumMarriageAgeMale);
            minAgeFemale = Math.Max(Config.MinFemaleMarriageAge, marriageModel.MinimumMarriageAgeFemale);
            maxAgeFemale = Math.Max(minAgeFemale + 1, Config.MaxFemaleMarriageAge);
            daysPerHumanYear = GetDaysPerHumanYear();

            sameKingdomOnly = !Config.AllowDiffKingdomSameCultureMarriage && !Config.AllowDiffKingdomDiffCultureMarriage;

            var trace = new List<string>
            {
                "Dynamic Parameters:",
                $"    Same-Kingdom Marriage Only? {sameKingdomOnly}",
                $"    Min. Age to Marry (Male):   {minAgeMale}",
                $"    Min. Age to Marry (Female): {minAgeFemale}",
                $"    Max. Age to Marry (Female): {maxAgeFemale}",
                $"    Days Per Human-Year:        {daysPerHumanYear}\n"
            };

            Util.Log.Print(trace);
        }

        private int GetDaysPerHumanYear()
        {
            int ret = daysPerHumanYearDefault;

            // Try to find Pacemaker.Export.GetDaysPerHumanYear(bool isAdult) from Pacemaker.dll
            var apiMethod = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(a => a.GetName().Name == "Pacemaker")?.ExportedTypes
                .SingleOrDefault(t => t.FullName == "Pacemaker.Export")?.GetMethod("GetDaysPerHumanYear");

            if (apiMethod is not null)
                ret = (int)Math.Round((float)apiMethod.Invoke(null, new object[] { true }));

            return ret <= 0 ? 1 : ret;
        }

        private DiplomacyModel? diplomacyModel;
        private MarriageModel? marriageModel;
        private bool sameKingdomOnly;
        private int minAgeMale;
        private int minAgeFemale;
        private int maxAgeFemale;
        private int daysPerHumanYear;

        private const int maxFemaleReproductionAge = 45;
        private const int daysPerHumanYearDefault = 21 * 4; // Vanilla timescale of 21 days/season
    }
}
