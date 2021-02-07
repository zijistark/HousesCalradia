using System;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

// TODO: Once e1.5.8 becomes stable, switch most of the RandomPick() calls to GetRandomElementWithPredicate()
//       No big deal, but using the predicate method on IReadOnlyList<T> is indeed a faster approach in many cases.

namespace HousesCalradia
{
    internal static class HeroUtil
    {
        public static Hero? SpawnNoble(Clan clan, int ageMin, int ageMax = -1, bool isFemale = false)
        {
            // Select a main template hero, which must be a Lord
            var mainTemplateSeq = Hero.All
                .Where(h => h.IsNoble
                    && h.CharacterObject.Occupation == Occupation.Lord
                    && h.IsFemale == isFemale);

            var mainTemplate = mainTemplateSeq.Where(h => h.Culture == clan.Culture).RandomPick()
                ?? mainTemplateSeq.RandomPick();

            if (mainTemplate is null)
                return null; // If we couldn't find a single one, we're screwed. Luckily this is basically a never-condition in any working setup.

            // Select a different auxiliary template to use for cross-pollination of randomized facial appearance, if possible.
            // The auxiliary template doesn't need to be a proper Lord.
            var auxTemplateSeq = Hero.All.Where(h => h.IsNoble && h != mainTemplate);

            var auxTemplate = auxTemplateSeq.Where(h => h.Culture == clan.Culture && h.IsFemale != isFemale).RandomPick()
                ?? auxTemplateSeq.Where(h => h.Culture == clan.Culture).RandomPick()
                ?? auxTemplateSeq.RandomPick()
                ?? mainTemplate;

            // Nail down that intended age
            ageMax = ageMax <= ageMin ? -1 : ageMax;
            int age = ageMax < 0 ? ageMin : MBRandom.RandomInt(ageMin, ageMax);

            // The juice: come to life!
            var hero = HeroCreator.CreateSpecialHero(mainTemplate.CharacterObject,
                bornSettlement: clan.HomeSettlement,
                faction: clan,
                supporterOfClan: clan,
                age: age);

            hero.Name = hero.FirstName;
            hero.IsNoble = true;

            // Randomize face/body parameters by simulating a cross between our main template
            // and our auxiliary template (might be same sex) + random mutation thrown into the mix
            RandomizeSpawnedNobleAppearance(hero, mainTemplate, auxTemplate);

            // Set attributes & skills & focus points

            // Currently I just ballpark skills and attributes such that they'll be decent but not beyond
            // the learning limit for the character. No special methodology here.

            // Attributes
            for (var attr = CharacterAttributesEnum.First; attr < CharacterAttributesEnum.End; ++attr)
                hero.SetAttributeValue(attr, MBRandom.RandomInt(6, 8));

            // Skills: level & focus point minimums
            const int MinRidingSkill = 100;

            foreach (var skillObj in Game.Current.SkillList)
            {
                var curSkill = hero.GetSkillValue(skillObj);
                var curFocus = hero.HeroDeveloper.GetFocus(skillObj);

                var (min, max) = (80, 160);

                if (skillObj == DefaultSkills.Riding)
                    min = MinRidingSkill;

                int minSkill = MBRandom.RandomInt(min, max);
                int minFocus = minSkill >= 120 ? 5 : minSkill >= 95 ? MBRandom.RandomInt(3, 4) : MBRandom.RandomInt(2, 4);

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
                    && c.IsFemale == isFemale).RandomPick();

            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c) && c.Culture == hero.Culture).RandomPick();
            equipSoldier ??= equipSoldierSeq.Where(c => c.Culture == hero.Culture && c.IsFemale == isFemale).RandomPick();
            equipSoldier ??= equipSoldierSeq.Where(c => c.Culture == hero.Culture).RandomPick();
            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c) && c.IsFemale == isFemale).RandomPick();
            equipSoldier ??= equipSoldierSeq.Where(c => TroopHasPreferredFormationClass(c)).RandomPick();
            equipSoldier ??= equipSoldierSeq.RandomPick();

#if STABLE
            if (equipSoldier?.BattleEquipments.RandomPick() is Equipment equip)
                hero.BattleEquipment.FillFrom(equip);
#else
            if (equipSoldier?.RandomBattleEquipment is Equipment equip)
                hero.BattleEquipment.FillFrom(equip);
#endif

            // All done!
            hero.ChangeState(Hero.CharacterStates.Active);
            CampaignEventDispatcher.Instance.OnHeroCreated(hero, false);
            return hero;
        }

        private delegate void StaticBodyPropertiesDelegate(Hero instance, StaticBodyProperties value);
        private static readonly Reflect.Setter<Hero> SetStaticBodyPropertiesRM = new("StaticBodyProperties");
        private static readonly StaticBodyPropertiesDelegate SetStaticBodyProperties = SetStaticBodyPropertiesRM.GetOpenDelegate<StaticBodyPropertiesDelegate>();

        private static void RandomizeSpawnedNobleAppearance(Hero hero, Hero mainTemplate, Hero auxTemplate)
        {
            // Note that this is adapted from how the mod Heritage from zenDzeeMods randomizes
            // FaceGen params with up to two seed template heroes (opposite-sex parents, in that case)

            // First some quick helper methods upon random slider value selection...

            static byte GetRandomSliderValue(byte v1, byte v2, byte extraRandomization = 4, byte maxValue = 0xF)
            {
                float diff = Math.Abs(v1 - v2) + extraRandomization + extraRandomization;
                float vx = diff * MBRandom.RandomFloat - extraRandomization;
                vx += Math.Min(v1, v2);
                return (byte)Math.Max(0, Math.Min(maxValue, vx));
            }

            static byte ChooseRandomSliderValue(byte v1, byte v2) => MBRandom.RandomFloat > 0.5 ? v1 : v2;

            // Initialize sliders (technically heroSliders doesn't need to be a value-copy of mainSliders, but w/e)
            var mainSliders = new StaticBodySliders(mainTemplate.BodyProperties.StaticProperties);
            var auxSliders = new StaticBodySliders(auxTemplate.BodyProperties.StaticProperties);
            var heroSliders = mainSliders.Copy();

            // The fuckin' magic:
            heroSliders.FaceAsymmetry = GetRandomSliderValue(mainSliders.FaceAsymmetry, auxSliders.FaceAsymmetry);
            heroSliders.FaceCenterHeight = GetRandomSliderValue(mainSliders.FaceCenterHeight, auxSliders.FaceCenterHeight);
            heroSliders.FaceCheekboneDepth = GetRandomSliderValue(mainSliders.FaceCheekboneDepth, auxSliders.FaceCheekboneDepth);
            heroSliders.FaceCheekboneHeight = GetRandomSliderValue(mainSliders.FaceCheekboneHeight, auxSliders.FaceCheekboneHeight);
            heroSliders.FaceCheekboneWidth = GetRandomSliderValue(mainSliders.FaceCheekboneWidth, auxSliders.FaceCheekboneWidth);
            heroSliders.FaceDepth = GetRandomSliderValue(mainSliders.FaceDepth, auxSliders.FaceDepth);
            heroSliders.FaceEarSize = GetRandomSliderValue(mainSliders.FaceEarSize, auxSliders.FaceEarSize);
            heroSliders.FaceEyeSocketSize = GetRandomSliderValue(mainSliders.FaceEyeSocketSize, auxSliders.FaceEyeSocketSize);
            heroSliders.FaceRatio = GetRandomSliderValue(mainSliders.FaceRatio, auxSliders.FaceRatio);
            heroSliders.FaceSharpness = GetRandomSliderValue(mainSliders.FaceSharpness, auxSliders.FaceSharpness);
            heroSliders.FaceTempleWidth = GetRandomSliderValue(mainSliders.FaceTempleWidth, auxSliders.FaceTempleWidth);
            heroSliders.FaceWeight = GetRandomSliderValue(mainSliders.FaceWeight, auxSliders.FaceWeight);
            heroSliders.FaceWidth = GetRandomSliderValue(mainSliders.FaceWidth, auxSliders.FaceWidth);
            heroSliders.EyeAsymmetry = GetRandomSliderValue(mainSliders.EyeAsymmetry, auxSliders.EyeAsymmetry);
            heroSliders.EyeBrowInnerHeight = GetRandomSliderValue(mainSliders.EyeBrowInnerHeight, auxSliders.EyeBrowInnerHeight);
            heroSliders.EyeBrowMiddleHeight = GetRandomSliderValue(mainSliders.EyeBrowMiddleHeight, auxSliders.EyeBrowMiddleHeight);
            heroSliders.EyeDepth = GetRandomSliderValue(mainSliders.EyeDepth, auxSliders.EyeDepth);
            heroSliders.EyeEyebrowDepth = GetRandomSliderValue(mainSliders.EyeEyebrowDepth, auxSliders.EyeEyebrowDepth);
            heroSliders.EyeEyelidHeight = GetRandomSliderValue(mainSliders.EyeEyelidHeight, auxSliders.EyeEyelidHeight);
            heroSliders.EyeInnerHeight = GetRandomSliderValue(mainSliders.EyeInnerHeight, auxSliders.EyeInnerHeight);
            heroSliders.EyeMonolidEyes = GetRandomSliderValue(mainSliders.EyeMonolidEyes, auxSliders.EyeMonolidEyes);
            heroSliders.EyeOuterHeight = GetRandomSliderValue(mainSliders.EyeOuterHeight, auxSliders.EyeOuterHeight);
            heroSliders.EyePosition = GetRandomSliderValue(mainSliders.EyePosition, auxSliders.EyePosition);
            heroSliders.EyeSize = GetRandomSliderValue(mainSliders.EyeSize, auxSliders.EyeSize, 1);
            heroSliders.EyeToEyeDistance = GetRandomSliderValue(mainSliders.EyeToEyeDistance, auxSliders.EyeToEyeDistance, 1);
            heroSliders.NoseAngle = GetRandomSliderValue(mainSliders.NoseAngle, auxSliders.NoseAngle, 1);
            heroSliders.NoseAsymmetry = GetRandomSliderValue(mainSliders.NoseAsymmetry, auxSliders.NoseAsymmetry, 1);
            heroSliders.NoseBridge = GetRandomSliderValue(mainSliders.NoseBridge, auxSliders.NoseBridge, 1);
            heroSliders.NoseBump = GetRandomSliderValue(mainSliders.NoseBump, auxSliders.NoseBump, 1);
            heroSliders.NoseDefenition = GetRandomSliderValue(mainSliders.NoseDefenition, auxSliders.NoseDefenition, 1);
            heroSliders.NoseLength = GetRandomSliderValue(mainSliders.NoseLength, auxSliders.NoseLength, 1);
            heroSliders.NoseNostrilHeight = GetRandomSliderValue(mainSliders.NoseNostrilHeight, auxSliders.NoseNostrilHeight, 1);
            heroSliders.NoseNostrilSize = GetRandomSliderValue(mainSliders.NoseNostrilSize, auxSliders.NoseNostrilSize, 1);
            heroSliders.NoseSize = GetRandomSliderValue(mainSliders.NoseSize, auxSliders.NoseSize, 1);
            heroSliders.NoseTipHeight = GetRandomSliderValue(mainSliders.NoseTipHeight, auxSliders.NoseTipHeight, 1);
            heroSliders.NoseWidth = GetRandomSliderValue(mainSliders.NoseWidth, auxSliders.NoseWidth, 1);
            heroSliders.MouthChinForward = GetRandomSliderValue(mainSliders.MouthChinForward, auxSliders.MouthChinForward, 2);
            heroSliders.MouthChinLength = GetRandomSliderValue(mainSliders.MouthChinLength, auxSliders.MouthChinLength, 2);
            heroSliders.MouthForward = GetRandomSliderValue(mainSliders.MouthForward, auxSliders.MouthForward, 2);
            heroSliders.MouthFrowSmile = GetRandomSliderValue(mainSliders.MouthFrowSmile, auxSliders.MouthFrowSmile);
            heroSliders.MouthJawHeight = GetRandomSliderValue(mainSliders.MouthJawHeight, auxSliders.MouthJawHeight);
            heroSliders.MouthJawLine = GetRandomSliderValue(mainSliders.MouthJawLine, auxSliders.MouthJawLine, 2);
            heroSliders.MouthLipsConcaveConvex = GetRandomSliderValue(mainSliders.MouthLipsConcaveConvex, auxSliders.MouthLipsConcaveConvex);
            heroSliders.MouthLipThickness = GetRandomSliderValue(mainSliders.MouthLipThickness, auxSliders.MouthLipThickness);
            heroSliders.MouthPosition = GetRandomSliderValue(mainSliders.MouthPosition, auxSliders.MouthPosition, 2);
            heroSliders.MouthTeethType = GetRandomSliderValue(mainSliders.MouthTeethType, auxSliders.MouthTeethType);
            heroSliders.MouthWidth = GetRandomSliderValue(mainSliders.MouthWidth, auxSliders.MouthWidth);

            heroSliders.EyeColor = ChooseRandomSliderValue(mainSliders.EyeColor, auxSliders.EyeColor);
            heroSliders.EyeShape = ChooseRandomSliderValue(mainSliders.EyeShape, auxSliders.EyeShape);
            heroSliders.FaceEarShape = ChooseRandomSliderValue(mainSliders.FaceEarShape, auxSliders.FaceEarShape);
            heroSliders.MouthBottomLipShape = ChooseRandomSliderValue(mainSliders.MouthBottomLipShape, auxSliders.MouthBottomLipShape);
            heroSliders.MouthChinShape = ChooseRandomSliderValue(mainSliders.MouthChinShape, auxSliders.MouthChinShape);
            heroSliders.MouthJawShape = ChooseRandomSliderValue(mainSliders.MouthJawShape, auxSliders.MouthJawShape);
            heroSliders.MouthTopLipShape = ChooseRandomSliderValue(mainSliders.MouthTopLipShape, auxSliders.MouthTopLipShape);
            heroSliders.NoseShape = ChooseRandomSliderValue(mainSliders.NoseShape, auxSliders.NoseShape);
            heroSliders.HairColor = ChooseRandomSliderValue(mainSliders.HairColor, auxSliders.HairColor);
            heroSliders.SkinColor = ChooseRandomSliderValue(mainSliders.SkinColor, auxSliders.SkinColor);

            heroSliders.HairType = GetRandomSliderValue(0, StaticBodySliders.MaxHairType(hero.IsFemale), 0, StaticBodySliders.MaxHairType(hero.IsFemale));

            heroSliders.MarkingsColor = 0;
            heroSliders.MarkingsType = 0;

            // Commit the new body properties, and we're done!
            SetStaticBodyProperties(hero, heroSliders.GetStaticBodyProperties());
        }
    }
}
