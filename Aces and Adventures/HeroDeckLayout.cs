public class HeroDeckLayout : ADeckLayout<HeroDeckPile, Ability>
{
	public ACardLayout draw;

	public ACardLayout selectionHand;

	public ACardLayout discard;

	public ACardLayout selectionHandUnrestricted;

	protected override ACardLayout this[HeroDeckPile? pile]
	{
		get
		{
			return pile switch
			{
				HeroDeckPile.Draw => draw, 
				HeroDeckPile.SelectionHand => selectionHand, 
				HeroDeckPile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case HeroDeckPile.Draw:
					draw = value;
					break;
				case HeroDeckPile.SelectionHand:
					selectionHand = value;
					break;
				case HeroDeckPile.Discard:
					discard = value;
					break;
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(Ability value)
	{
		return AbilityCardView.Create(value);
	}
}
