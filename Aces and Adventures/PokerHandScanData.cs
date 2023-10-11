using System.Collections.Generic;

public class PokerHandScanData
{
	private PoolKeepItemListHandle<PlayingCard> _sortedCards;

	private PoolDictionaryValuesHandle<PlayingCardValue, List<PlayingCard>> _cardsByValue;

	private PoolDictionaryValuesHandle<PlayingCardSuit, List<PlayingCard>> _cardsBySuit;

	public int count => sortedCards.Count;

	public List<PlayingCard> sortedCards => _sortedCards;

	public Dictionary<PlayingCardValue, List<PlayingCard>> cardsByValue => _cardsByValue;

	public Dictionary<PlayingCardSuit, List<PlayingCard>> cardsBySuit => _cardsBySuit;

	static PokerHandScanData()
	{
		Pools.CreatePoolList<PlayingCard>();
	}

	public void OnUnpool()
	{
	}

	public void Clear()
	{
		Pools.Repool(ref _sortedCards);
		Pools.Repool(ref _cardsByValue);
		Pools.Repool(ref _cardsBySuit);
	}

	public PokerHandScanData SetCards(IEnumerable<PlayingCard> cards)
	{
		_sortedCards = Pools.UseKeepItemList(cards);
		sortedCards.Sort(PlayingCard.DescendingComparer.Default);
		_cardsByValue = Pools.UseDictionaryValues<PlayingCardValue, List<PlayingCard>>();
		_cardsBySuit = Pools.UseDictionaryValues<PlayingCardSuit, List<PlayingCard>>();
		foreach (PlayingCard sortedCard in sortedCards)
		{
			if (!cardsByValue.ContainsKey(sortedCard))
			{
				cardsByValue.Add(sortedCard, Pools.Unpool<List<PlayingCard>>());
			}
			if (!cardsBySuit.ContainsKey(sortedCard))
			{
				cardsBySuit.Add(sortedCard, Pools.Unpool<List<PlayingCard>>());
			}
			cardsByValue[sortedCard].Add(sortedCard);
			cardsBySuit[sortedCard].Add(sortedCard);
		}
		return this;
	}
}
