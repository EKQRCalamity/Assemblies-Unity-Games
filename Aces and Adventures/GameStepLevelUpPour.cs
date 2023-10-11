using System.Collections;
using UnityEngine;

public class GameStepLevelUpPour : AGameStepLevelUp
{
	private bool _confirmPressed;

	protected override void _OnConfirmPressed()
	{
		_confirmPressed = true;
	}

	public override void Start()
	{
		if (base.quickLevelUp)
		{
			_OnConfirmPressed();
		}
	}

	protected override IEnumerator Update()
	{
		while (!base.levelUpView.main.GetLayout(LevelUpPile.VialPour).IsAtRest())
		{
			yield return null;
		}
		base.vial.vialview.bottomCorkEnabled = false;
		GameStepProjectileMedia fillStep = ((!base.quickLevelUp) ? (base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.vialToPlant, base.vial, base.plant)) as GameStepProjectileMedia) : null);
		LevelUpLeaf newLeaf = base.plant.leafs.Add(new LevelUpLeaf(LevelUpLeaf.State.New));
		while (base.vial.vialview.barFiller.normalizedValue > 0f)
		{
			base.vial.vialview.barFiller.normalizedValue -= Time.deltaTime * _confirmPressed.ToFloat(100f, 1f) / GameVariables.Values.levelUp.vialToPlantPourTime;
			newLeaf.leafView.barFiller.normalizedValue = 1f - base.vial.vialview.barFiller.normalizedValue;
			yield return null;
		}
		fillStep?.Stop();
		base.levelUp.main.Transfer(base.vial, LevelUpPile.VialDiscard);
		base.levelUp.main.Transfer(newLeaf, LevelUpPile.LeafExit);
		base.plant.leafs.Add(new LevelUpLeaf());
		while (!base.levelUpView.main.GetLayout(LevelUpPile.LeafExit).IsAtRest())
		{
			yield return null;
		}
		TransitionTo(new GameStepLevelUpPresentReward());
	}
}
