using System;

public class AbilityDeckLayout : ADeckLayout<Ability.Pile, Ability>
{
	public ACardLayout draw;

	public ACardLayout hand;

	public ACardLayout activationHand;

	public ACardLayout activationHandWaiting;

	public ACardLayout hero;

	public ACardLayout heroPassive;

	public ACardLayout select;

	public ACardLayout discard;

	public ACardLayout inspectDraw;

	protected override ACardLayout this[Ability.Pile? pile]
	{
		get
		{
			return pile switch
			{
				Ability.Pile.Draw => draw, 
				Ability.Pile.Hand => hand, 
				Ability.Pile.ActivationHand => activationHand, 
				Ability.Pile.ActivationHandWaiting => activationHandWaiting, 
				Ability.Pile.HeroAct => hero, 
				Ability.Pile.HeroPassive => heroPassive, 
				Ability.Pile.Discard => discard, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case Ability.Pile.Draw:
					draw = value;
					break;
				case Ability.Pile.Hand:
					hand = value;
					break;
				case Ability.Pile.ActivationHand:
					activationHand = value;
					break;
				case Ability.Pile.ActivationHandWaiting:
					activationHandWaiting = value;
					break;
				case Ability.Pile.HeroAct:
					hero = value;
					break;
				case Ability.Pile.HeroPassive:
					heroPassive = value;
					break;
				case Ability.Pile.Discard:
					discard = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(Ability value)
	{
		return AbilityCardView.Create(value);
	}
}
