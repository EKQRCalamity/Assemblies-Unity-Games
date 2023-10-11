public class TurnOrderSpaceLayout : ADeckLayout<TurnOrderSpace.Pile, TurnOrderSpace>
{
	public ACardLayout inactive;

	public ACardLayout active;

	protected override ACardLayout this[TurnOrderSpace.Pile? pile]
	{
		get
		{
			return pile switch
			{
				TurnOrderSpace.Pile.Inactive => inactive, 
				TurnOrderSpace.Pile.Active => active, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case TurnOrderSpace.Pile.Inactive:
					inactive = value;
					break;
				case TurnOrderSpace.Pile.Active:
					active = value;
					break;
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(TurnOrderSpace value)
	{
		return TurnOrderSpaceView.Create(value);
	}

	public CardLayoutElement FirstActive()
	{
		return base.deck.FirstInPile(TurnOrderSpace.Pile.Active)?.view;
	}
}
