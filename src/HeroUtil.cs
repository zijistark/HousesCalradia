using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace HousesCalradia
{
	public static class HeroUtil
	{
		public static Hero SpawnNoble(Clan clan, int ageMin, int ageMax = -1, bool isFemale = false)
		{
			var templateSeq = Hero.All.Where(h =>
				h.IsNoble &&
				h.CharacterObject.Occupation == Occupation.Lord &&
				((isFemale && h.IsFemale) || (!isFemale && !h.IsFemale)));

			var template = templateSeq.Where(h => h.Culture == clan.Culture).GetRandomElement() ?? templateSeq.GetRandomElement();

			if (template == null)
				return null;

			ageMax = ageMax <= ageMin ? -1 : ageMax;
			int age = ageMax < 0 ? ageMin : MBRandom.RandomInt(ageMin, ageMax);

			var hero = HeroCreator.CreateSpecialHero(template.CharacterObject,
				bornSettlement: clan.HomeSettlement,
				faction: clan,
				supporterOfClan: clan,
				age: age);

			// Our own, exact age assignment:
			hero.BirthDay = CampaignTime.Now - CampaignTime.Years(age);
			hero.CharacterObject.Age = hero.Age; // Get it into the BasicCharacterObject.Age property as well

			// TODO: Based upon age, assign her attributes and skills randomly with 2 random attributes getting higher weights for skill specialization

			// TODO: Assign random traits

			hero.Name = hero.FirstName;
			hero.IsNoble = true;
			hero.ChangeState(Hero.CharacterStates.Active);
			CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
			return hero;
		}
	}
}
