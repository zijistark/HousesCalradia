using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HousesCalradia
{
	public class SubModule : MBSubModuleBase
	{
		/* Semantic Versioning (https://semver.org)
		 *
		 * In the case of your usual mod, "breaking API compatibility" (major version increment) is save-incompatibility
		 * with itself with respect to the prior version. However, if the mod talks to others, that's an actual API too.
		 * The rest of the version components function as usual.
		 */
		public const int SemVerMajor = 1;
		public const int SemVerMinor = 0;
		public const int SemVerPatch = 2;
		public const string SemVerSpecial = null;
		private static readonly string SemVerEnd = (SemVerSpecial != null) ? '-' + SemVerSpecial : string.Empty;
		public static readonly string Version = $"{SemVerMajor}.{SemVerMinor}.{SemVerPatch}{SemVerEnd}";

		public static readonly string Name = typeof(SubModule).Namespace;
		public static readonly string DisplayName = "Houses of Calradia"; // to be shown to humans in-game
		public static readonly string HarmonyDomain = "com.zijistark.bannerlord." + Name.ToLower();

		internal static readonly Color ImportantTextColor = Color.FromUint(0x00F16D26); // orange

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			Util.EnableLog = true; // enable various debug logging
			Util.EnableTracer = true; // enable code event tracing (requires enabled logging)
		}

		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			base.OnBeforeInitialModuleScreenSetAsRoot();

			if (!hasLoaded)
			{
				var harmony = new Harmony(HarmonyDomain);
				harmony.PatchAll();

				InformationManager.DisplayMessage(new InformationMessage($"Loaded {DisplayName}", ImportantTextColor));
				hasLoaded = true;
			}
		}

		protected override void OnGameStart(Game game, IGameStarter starterObject)
		{
			base.OnGameStart(game, starterObject);

			if (game.GameType is Campaign)
			{
				CampaignGameStarter initializer = (CampaignGameStarter)starterObject;
				AddBehaviors(initializer);
			}
		}

		protected void AddBehaviors(CampaignGameStarter gameInitializer) =>	gameInitializer.AddBehavior(new MarriageBehavior());

		private bool hasLoaded = false;
	}
}
