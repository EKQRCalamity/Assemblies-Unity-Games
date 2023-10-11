using System;

public class ResourceDeckLayout : ADeckLayout<ResourceCard.Pile, ResourceCard>
{
	public ACardLayout draw;

	public ACardLayout hand;

	public ACardLayout activationHand;

	public ACardLayout activationHandWaiting;

	public ACardLayout combatHand;

	public ACardLayout topDeckHand;

	public ACardLayout select;

	public ACardLayout discard;

	protected override ACardLayout this[ResourceCard.Pile? pile]
	{
		get
		{
			return pile switch
			{
				ResourceCard.Pile.DrawPile => draw, 
				ResourceCard.Pile.Hand => hand, 
				ResourceCard.Pile.ActivationHand => activationHand, 
				ResourceCard.Pile.ActivationHandWaiting => activationHandWaiting, 
				ResourceCard.Pile.AttackHand => combatHand, 
				ResourceCard.Pile.DefenseHand => combatHand, 
				ResourceCard.Pile.TopDeckHand => topDeckHand, 
				ResourceCard.Pile.DiscardPile => discard, 
				null => null, 
				_ => throw new ArgumentOutOfRangeException("pile", pile, null), 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case ResourceCard.Pile.DrawPile:
					draw = value;
					break;
				case ResourceCard.Pile.Hand:
					hand = value;
					break;
				case ResourceCard.Pile.ActivationHand:
					activationHand = value;
					break;
				case ResourceCard.Pile.ActivationHandWaiting:
					activationHandWaiting = value;
					break;
				case ResourceCard.Pile.AttackHand:
					combatHand = value;
					break;
				case ResourceCard.Pile.DefenseHand:
					combatHand = value;
					break;
				case ResourceCard.Pile.TopDeckHand:
					topDeckHand = value;
					break;
				case ResourceCard.Pile.DiscardPile:
					discard = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(ResourceCard value)
	{
		return ResourceCardView.Create(value);
	}
}
