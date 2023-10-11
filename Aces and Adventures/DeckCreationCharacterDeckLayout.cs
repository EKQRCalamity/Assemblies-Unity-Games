using System.Collections.Generic;

public class DeckCreationCharacterDeckLayout : ADeckLayout<DeckCreationPile, Player>
{
	public ACardLayout draw;

	public ACardLayout results;

	public ACardLayout list;

	public ACardLayout discard;

	protected override ACardLayout this[DeckCreationPile? pile]
	{
		get
		{
			return pile switch
			{
				DeckCreationPile.Draw => draw, 
				DeckCreationPile.Results => results, 
				DeckCreationPile.List => list, 
				DeckCreationPile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case DeckCreationPile.Draw:
					draw = value;
					break;
				case DeckCreationPile.Results:
					results = value;
					break;
				case DeckCreationPile.List:
					list = value;
					break;
				case DeckCreationPile.Discard:
					discard = value;
					break;
				}
			}
		}
	}

	private void Awake()
	{
		List<DeckCreationPile> list2 = (base.faceUpStacks = new List<DeckCreationPile>());
		base.faceDownStacks = list2;
	}

	protected override CardLayoutElement _CreateView(Player value)
	{
		return AdventureTargetView.Create(value);
	}
}
