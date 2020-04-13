using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace ExampleSubModule
{
	public class ExampleMapVisibilityModel : DefaultMapVisibilityModel
	{
		public override float GetPartySpottingRange(MobileParty party, StatExplainer explainer)
		{
			return 100f;
		}
	}
}
