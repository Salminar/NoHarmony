using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace ExampleSubModule
{
	public class ExamplePersuasionModel : DefaultPersuasionModel
	{
		public override void GetChances(PersuasionOptionArgs optionArgs, out float successChance, out float critSuccessChance, out float critFailChance, out float failChance, float difficultyMultiplier)
		{
			successChance = 1f;
			critSuccessChance = 1f;
			critFailChance = 0f;
			failChance = 0f;
		}
	}
}
