using System;

public class AdventureDeckLayout : ADeckLayout<AdventureCard.Pile, ATarget>
{
	public ACardLayout draw;

	public ACardLayout selectionHand;

	public ACardLayout activeHand;

	public ACardLayout turnOrder;

	public ACardLayout itemActHand;

	public ACardLayout itemTraitHand;

	public ACardLayout discard;

	public ACardLayout inspectHand;

	public CardHandLayoutSettings inspectSettings;

	public CardHandLayoutSettings inspectLargeSettings;

	public CardHandLayoutSettings inspectSmallSettings;

	public CardHandLayoutSettings inspectHugeGroupSettings;

	protected override ACardLayout this[AdventureCard.Pile? pile]
	{
		get
		{
			return pile switch
			{
				AdventureCard.Pile.Draw => draw, 
				AdventureCard.Pile.SelectionHand => selectionHand, 
				AdventureCard.Pile.ActiveHand => activeHand, 
				AdventureCard.Pile.TurnOrder => turnOrder, 
				AdventureCard.Pile.ItemActHand => itemActHand, 
				AdventureCard.Pile.ItemTraitHand => itemTraitHand, 
				AdventureCard.Pile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case AdventureCard.Pile.Draw:
					draw = value;
					break;
				case AdventureCard.Pile.SelectionHand:
					selectionHand = value;
					break;
				case AdventureCard.Pile.ActiveHand:
					activeHand = value;
					break;
				case AdventureCard.Pile.TurnOrder:
					turnOrder = value;
					break;
				case AdventureCard.Pile.ItemActHand:
					itemActHand = value;
					break;
				case AdventureCard.Pile.ItemTraitHand:
					itemTraitHand = value;
					break;
				case AdventureCard.Pile.Discard:
					discard = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(ATarget value)
	{
		return AdventureTargetView.Create(value);
	}
}
