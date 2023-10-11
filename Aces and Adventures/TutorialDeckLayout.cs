public class TutorialDeckLayout : ADeckLayout<TutorialCard.Pile, TutorialCard>
{
	public ACardLayout draw;

	public ACardLayout topLeft;

	public ACardLayout discard;

	protected override ACardLayout this[TutorialCard.Pile? pile]
	{
		get
		{
			return pile switch
			{
				TutorialCard.Pile.Draw => draw, 
				TutorialCard.Pile.TopLeft => topLeft, 
				TutorialCard.Pile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case TutorialCard.Pile.Draw:
					draw = value;
					break;
				case TutorialCard.Pile.TopLeft:
					topLeft = value;
					break;
				case TutorialCard.Pile.Discard:
					discard = value;
					break;
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(TutorialCard value)
	{
		return TutorialCardView.Create(value);
	}
}
