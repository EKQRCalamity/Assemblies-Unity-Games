using System;
using System.Collections;
using System.Collections.Generic;

public class GameStepAddCardsOverTime<K, V> : GameStep where K : struct, IConvertible where V : ATarget
{
	private IdDeck<K, V> _deck;

	private IEnumerable<V> _cards;

	private K? _toPile;

	private int _cardsPerFrame;

	private int _cardsRemainingInFrame;

	public GameStepAddCardsOverTime(IdDeck<K, V> deck, IEnumerable<V> cards, K? toPile = null, int cardsPerFrame = 1)
	{
		_deck = deck;
		_cards = cards;
		_toPile = toPile;
		_cardsPerFrame = (_cardsRemainingInFrame = cardsPerFrame);
	}

	protected override IEnumerator Update()
	{
		foreach (V card in _cards)
		{
			_deck.Add(card, _toPile);
			if (--_cardsRemainingInFrame <= 0)
			{
				yield return null;
				_cardsRemainingInFrame = _cardsPerFrame;
			}
		}
	}
}
