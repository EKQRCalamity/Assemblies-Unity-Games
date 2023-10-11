using System.Collections.Generic;

public class ButtonDeckLayout : ADeckLayout<ButtonCard.Pile, ButtonCard>
{
	public ACardLayout inactive;

	public ACardLayout active;

	private Dictionary<ButtonCardType, Id<ButtonCard>> _map = new Dictionary<ButtonCardType, Id<ButtonCard>>();

	protected override ACardLayout this[ButtonCard.Pile? pile]
	{
		get
		{
			return pile switch
			{
				ButtonCard.Pile.Inactive => inactive, 
				ButtonCard.Pile.Active => active, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case ButtonCard.Pile.Inactive:
					inactive = value;
					break;
				case ButtonCard.Pile.Active:
					active = value;
					break;
				}
			}
		}
	}

	public ButtonCard this[ButtonCardType type]
	{
		get
		{
			ButtonCard buttonCard = _map.GetValueOrDefault(type).value;
			if (buttonCard == null)
			{
				Id<ButtonCard> id2 = (_map[type] = base.deck.Add(new ButtonCard(type)));
				buttonCard = id2;
			}
			return buttonCard;
		}
	}

	protected override CardLayoutElement _CreateView(ButtonCard value)
	{
		Id<ButtonCard> id2 = (_map[value] = value);
		return ButtonCardView.Create(id2);
	}

	public void Transfer(ButtonCardType type, ButtonCard.Pile pile)
	{
		base.deck.Transfer(this[type], pile);
	}

	public void Activate(ButtonCardType type)
	{
		Transfer(type, ButtonCard.Pile.Active);
	}

	public void Deactivate(ButtonCardType type)
	{
		Transfer(type, ButtonCard.Pile.Inactive);
	}

	public void SetActive(ButtonCardType type, bool setActive, bool forceUpdateCancelStone = false)
	{
		Transfer(type, setActive ? ButtonCard.Pile.Active : ButtonCard.Pile.Inactive);
		if (forceUpdateCancelStone)
		{
			GameStateView.Instance?.ForceUpdateCancelStone();
		}
	}
}
