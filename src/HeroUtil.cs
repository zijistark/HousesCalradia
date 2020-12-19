using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace HousesCalradia
{
    internal static class HeroUtil
    {
        public static Hero? SpawnNoble(Clan clan, int ageMin, int ageMax = -1, bool isFemale = false)
        {
            var templateSeq = Hero.All
                .Where(h => h.IsNoble
                    && h.CharacterObject.Occupation == Occupation.Lord
                    && h.IsFemale == isFemale);

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

            // Attributes
            for (var attr = CharacterAttributesEnum.First; attr < CharacterAttributesEnum.End; ++attr)
                hero.SetAttributeValue(attr, MBRandom.RandomInt(6, 7));

            const int MinRidingSkill = 100;

            // Skills: levels & focus point minimums
            foreach (var skillObj in Game.Current.SkillList)
            {
                var curSkill = hero.GetSkillValue(skillObj);
                var curFocus = hero.HeroDeveloper.GetFocus(skillObj);

                var (min, max) = (75, 125);

                if (skillObj == DefaultSkills.Riding)
                    min = MinRidingSkill;

                int minSkill = MBRandom.RandomInt(min, max);
                int minFocus = minSkill >= 100 ? 4 : MBRandom.RandomInt(2, 3);

                if (curSkill < minSkill)
                    hero.HeroDeveloper.ChangeSkillLevel(skillObj, minSkill - curSkill, false);

                if (curFocus < minFocus)
                    hero.HeroDeveloper.AddFocus(skillObj, minFocus - curFocus, false);
            }

            // Find a high-tier cavalry-based soldier from which to template the new hero's BattleEquipment,
            // preferably with the same gender (though it usually doesn't matter too much in armor, it can),
            // and of course preferably with the same culture as the new hero.
            static bool TroopHasPreferredFormationClass(CharacterObject c)
            {
                return c.DefaultFormationClass == FormationClass.HeavyCavalry
                    || c.DefaultFormationClass == FormationClass.Cavalry
                    || c.DefaultFormationClass == FormationClass.HorseArcher
                    || c.DefaultFormationClass == FormationClass.LightCavalry;
            }

            var equipSoldierSeq = CharacterObject.All.Where(c => c.Occupation == Occupation.Soldier && c.Tier >= 5);

            var equipSoldier = equipSoldierSeq
                .Where(c => TroopHasPreferredFormationClass(c)
                    && c.Culture == hero.Culture
                    && c.IsFemale == isFemale).GetRandomElement();

            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c) && c.Culture == hero.Culture).GetRandomElement();
            equipSoldier ??= equipSoldierSeq.Where(c => c.Culture == hero.Culture && c.IsFemale == isFemale).GetRandomElement();
            equipSoldier ??= equipSoldierSeq.Where(c => c.Culture == hero.Culture).GetRandomElement();
            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c) && c.IsFemale == isFemale).GetRandomElement();
            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c)).GetRandomElement();
            equipSoldier ??= equipSoldierSeq.GetRandomElement();

            if (equipSoldier?.BattleEquipments.GetRandomElement() is { } equip)
                hero.BattleEquipment.FillFrom(equip);

            // TODO:
            // - morph StaticBodyParameters a bit, in a way that doesn't result in ogres

            hero.Name = hero.FirstName;
            hero.IsNoble = true;
            hero.ChangeState(Hero.CharacterStates.Active);
            CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
            return hero;
        }
    }
}
