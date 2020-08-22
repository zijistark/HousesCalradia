using System;
using TaleWorlds.CampaignSystem;

namespace HousesCalradia
{
	class MarriageBehavior : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		protected void OnSessionLaunched(CampaignGameStarter starter)
		{
		}

		protected void OnDailyTick()
		{
		}
	}
}
