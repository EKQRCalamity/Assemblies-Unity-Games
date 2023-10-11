public class CardIgnoreLayout : ACardLayout
{
	protected override CardLayoutElement.Target _GetLayoutTarget(CardLayoutElement card, int cardIndex, int cardCount)
	{
		return default(CardLayoutElement.Target);
	}

	protected override void Update()
	{
	}
}
