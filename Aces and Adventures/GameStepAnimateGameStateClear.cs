using System.Collections;

public class GameStepAnimateGameStateClear : GameStep
{
	private void _OnExileRest(ExilePile pile, ATarget card)
	{
		card?.view?.gameObject?.SetActive(value: false);
	}

	protected override void OnFirstEnabled()
	{
		base.view.exileDeckLayout.onRest += _OnExileRest;
	}

	public override void Start()
	{
		base.state.mapDeck.GetCardsSafe(ProceduralMap.Pile.Closed).AsEnumerable().EffectAll(delegate(ProceduralMap map)
		{
			map.view.DestroyCard();
		});
	}

	protected override IEnumerator Update()
	{
		base.view.ClearMessage();
		ACardLayout exileLayout = base.state.exileDeck.layout.GetLayout(ExilePile.ClearGameState);
		bool transferringCards = true;
		while (transferringCards)
		{
			transferringCards = false;
			foreach (ADeckLayoutBase deck in base.view.decks)
			{
				foreach (ATarget nextInPile in deck.GetNextInPiles())
				{
					if ((bool)nextInPile.view?.layout && nextInPile.view?.layout != exileLayout)
					{
						bool flag;
						transferringCards = (flag = true);
						if (flag)
						{
							base.state.exileDeck.Transfer(nextInPile, ExilePile.ClearGameState);
							nextInPile.view.ClearExitTransitions();
						}
					}
				}
			}
			yield return null;
		}
		while (!base.view.exileDeckLayout.GetLayout(ExilePile.ClearGameState).IsAtRest())
		{
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.view.exileDeckLayout.onRest -= _OnExileRest;
	}
}
