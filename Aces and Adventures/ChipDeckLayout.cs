using System.Linq;

public class ChipDeckLayout : ADeckLayout<Chip.Pile, Chip>
{
	public ACardLayout inactive;

	public ACardLayout attack;

	public ACardLayout activeAttack;

	protected override ACardLayout this[Chip.Pile? pile]
	{
		get
		{
			return pile switch
			{
				Chip.Pile.Inactive => inactive, 
				Chip.Pile.Attack => attack, 
				Chip.Pile.ActiveAttack => activeAttack, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case Chip.Pile.Inactive:
					inactive = value;
					break;
				case Chip.Pile.Attack:
					attack = value;
					break;
				case Chip.Pile.ActiveAttack:
					activeAttack = value;
					break;
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(Chip value)
	{
		return ChipView.Create(value);
	}

	public void SetCountInPile(Chip.Pile pile, ChipType chipType, int count)
	{
		int num = base.deck.GetCards(pile).Count((Chip chip) => (ChipType)chip == chipType);
		if (num < count)
		{
			foreach (Chip item in (from chip in base.deck.GetCards(Chip.Pile.Inactive)
				where (ChipType)chip == chipType
				select chip).Reverse().Take(count - num))
			{
				base.deck.Transfer(item, pile);
			}
			return;
		}
		if (num <= count)
		{
			return;
		}
		foreach (Chip item2 in (from chip in base.deck.GetCards(pile)
			where (ChipType)chip == chipType
			select chip).Reverse().Take(num - count))
		{
			base.deck.Transfer(item2, Chip.Pile.Inactive);
		}
	}
}
