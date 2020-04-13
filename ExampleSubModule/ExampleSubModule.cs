using NoHarmony;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace ExampleSubModule
{
	public class ExampleSubModule : NoHarmonyLoader
	{
		public override void NoHarmonyInit()
		{
			Logging = false;
		}

		public override void NoHarmonyLoad()
		{
			ReplaceModel<ExampleSmithModel, DefaultSmithingModel>();
			ReplaceModel<ExamplePersuasionModel, DefaultPersuasionModel>();
			ReplaceModel<ExampleMapVisibilityModel, DefaultMapVisibilityModel>();
		}
	}
}
