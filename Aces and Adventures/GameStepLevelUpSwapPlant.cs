using System.Collections;

public class GameStepLevelUpSwapPlant : AGameStepLevelUp
{
	private ClassSeal _selectedSeal;

	public GameStepLevelUpSwapPlant(ClassSeal selectedSeal)
	{
		_selectedSeal = selectedSeal;
	}

	protected override IEnumerator Update()
	{
		GameStep swapCards = ParallelStep(new GameStepLevelUpSwapCards(_selectedSeal.character));
		base.levelUp.selectedCharacter = _selectedSeal.character;
		base.levelUp.main.TransferPile(LevelUpPile.ActiveSeal, LevelUpPile.Seals);
		base.levelUp.main.Transfer(_selectedSeal, LevelUpPile.ActiveSeal);
		base.levelUp.main.TransferPile(LevelUpPile.Pot, LevelUpPile.PotDiscard);
		while (!base.levelUpView.main.GetLayout(LevelUpPile.ActiveSeal).IsAtRest())
		{
			yield return null;
		}
		base.levelUp.main.Transfer(base.levelUp.main.Add(new LevelUpPlant(_selectedSeal.character), LevelUpPile.PotDraw), LevelUpPile.Pot);
		while (!base.levelUpView.main.GetLayout(LevelUpPile.Pot).IsAtRest())
		{
			yield return null;
		}
		while (swapCards.shouldUpdate)
		{
			yield return null;
		}
	}
}
