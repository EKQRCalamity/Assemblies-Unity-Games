public class AppliedAbilityDeckLayout : ADeckLayout<AppliedPile, Ability>
{
	public ACardLayout buff;

	public ACardLayout debuff;

	protected override ACardLayout this[AppliedPile? pile]
	{
		get
		{
			return pile switch
			{
				AppliedPile.Buff => buff, 
				AppliedPile.Debuff => debuff, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case AppliedPile.Buff:
					buff = value;
					break;
				case AppliedPile.Debuff:
					debuff = value;
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
