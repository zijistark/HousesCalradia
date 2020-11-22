using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace HousesCalradia
{
	internal static class HeroUtil
	{
		public static Hero? SpawnNoble(Clan clan, int ageMin, int ageMax = -1, bool isFemale = false)
		{
			var templateSeq = Hero.All.Where(h =>
				h.IsNoble &&
				h.CharacterObject.Occupation == Occupation.Lord &&
				(isFemale && h.IsFemale || !isFemale && !h.IsFemale));

			var template = templateSeq.Where(h => h.Culture == clan.Culture).GetRandomElement() ?? templateSeq.GetRandomElement();

			if (template is null)
				return null;

			ageMax = ageMax <= ageMin ? -1 : ageMax;
			int age = ageMax < 0 ? ageMin : MBRandom.RandomInt(ageMin, ageMax);

			var hero = HeroCreator.CreateSpecialHero(template.CharacterObject,
				bornSettlement: clan.HomeSettlement,
				faction: clan,
				supporterOfClan: clan,
				age: age);

			// Our own, exact age assignment:
			// FIXME: Will need update in e1.5.5
			hero.BirthDay = CampaignTime.Now - CampaignTime.Years(age);
			hero.CharacterObject.Age = hero.Age; // Get it into the BasicCharacterObject.Age property as well

			// Attributes
			for (var attr = CharacterAttributesEnum.First; attr < CharacterAttributesEnum.End; ++attr)
				hero.SetAttributeValue(attr, MBRandom.RandomInt(6, 7));

			// Skills: levels & focus point minimums
			foreach (var skillObj in Game.Current.SkillList)
			{
				var curSkill = hero.GetSkillValue(skillObj);
				var curFocus = hero.HeroDeveloper.GetFocus(skillObj);

				int minSkill = MBRandom.RandomInt(75, 110);
				int minFocus = minSkill > 95 ? 4 : MBRandom.RandomInt(2, 3);

				if (curSkill < minSkill)
					hero.HeroDeveloper.ChangeSkillLevel(skillObj, minSkill - curSkill, false);

				if (curFocus < minFocus)
					hero.HeroDeveloper.AddFocus(skillObj, minFocus - curFocus, false);
			}

			// TODO:
			// - morph StaticBodyParameters a bit, in a way that doesn't result in ogres
			// - equip them with a culture-appropriate horse and horse harness
			// - ensure they have some decent equipment (maybe pick a template soldier from each culture)

			hero.Name = hero.FirstName;
			hero.IsNoble = true;
			hero.ChangeState(Hero.CharacterStates.Active);
			CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
			return hero;
		}
	}
}
