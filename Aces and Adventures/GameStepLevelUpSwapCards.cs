using System.Collections;
using System.Collections.Generic;

public class GameStepLevelUpSwapCards : GameStep
{
	private DataRef<CharacterData> _selectedCharacter;

	private float _duration;

	public GameStepLevelUpSwapCards(DataRef<CharacterData> selectedCharacter, float duration = 1f)
	{
		_selectedCharacter = selectedCharacter;
		_duration = duration;
	}

	protected override IEnumerator Update()
	{
		base.state.levelUp.main.TransferPile(LevelUpPile.LevelUps, LevelUpPile.LevelUpsTransition);
		if ((bool)_selectedCharacter)
		{
			foreach (IEnumerable<ATarget> item in base.state.levelUp.CreateLevelUps(_selectedCharacter).Overtime(_duration, ProfileManager.progress.experience.read.GetLevel(_selectedCharacter) + 3))
			{
				foreach (ATarget item2 in item)
				{
					base.state.levelUp.main.Transfer(item2, LevelUpPile.LevelUps);
				}
				yield return null;
			}
		}
		while (!base.state.levelUp.main.layout.GetLayout(LevelUpPile.LevelUpsTransition).IsAtRest())
		{
			yield return null;
		}
		foreach (ATarget item3 in base.state.levelUp.main.GetCardsSafe(LevelUpPile.LevelUpsTransition))
		{
			item3.view.RepoolCard();
		}
	}
}
