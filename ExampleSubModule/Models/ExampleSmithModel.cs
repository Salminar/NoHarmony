using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;

namespace ExampleSubModule
{
	public class ExampleSmithModel : DefaultSmithingModel
	{
		public override int GetEnergyCostForRefining(ref Crafting.RefiningFormula refineFormula, Hero hero)
		{
			return 0;
		}

		public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
		{
			return 0;
		}

		public override int GetEnergyCostForSmelting(ItemObject item, Hero hero)
		{
			return 0;
		}

	}
}
