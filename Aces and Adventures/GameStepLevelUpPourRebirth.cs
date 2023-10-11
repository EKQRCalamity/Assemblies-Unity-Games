using System.Collections;
using UnityEngine;

public class GameStepLevelUpPourRebirth : AGameStepLevelUp
{
	protected override IEnumerator Update()
	{
		while (!base.levelUpView.main.GetLayout(LevelUpPile.VialPour).IsAtRest())
		{
			yield return null;
		}
		base.vial.vialview.bottomCorkEnabled = false;
		GameStepProjectileMedia fillStep = base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.vialToPlant, base.vial, base.plant)) as GameStepProjectileMedia;
		while (base.vial.vialview.barFiller.normalizedValue > 0f)
		{
			base.vial.vialview.barFiller.normalizedValue -= Time.deltaTime / GameVariables.Values.levelUp.vialToPlantPourTime;
			foreach (LevelUpLeaf card in base.plant.leafs.GetCards(LevelUpLeafPile.Main))
			{
				card.leafView.barFiller.normalizedValue = base.vial.vialview.barFiller.normalizedValue;
			}
			((base.plant.rebirth == 0) ? base.plant.plantView.onRebirthOneChange : base.plant.plantView.onRebirthTwoChange)?.Invoke(1f - base.vial.vialview.barFiller.normalizedValue);
			yield return null;
		}
		foreach (LevelUpLeaf item in base.plant.leafs.GetCardsSafe())
		{
			item.view.DestroyCard();
		}
		fillStep?.Stop();
		base.levelUp.main.Transfer(base.vial, LevelUpPile.VialDiscard);
		TransitionTo(new GameStepLevelUpPresentReward());
	}
}
