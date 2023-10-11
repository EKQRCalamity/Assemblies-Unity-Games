using System.Collections.Generic;

public class AbilityCategoryEqualityComparer : IEqualityComparer<CardLayoutElement>
{
	public static readonly AbilityCategoryEqualityComparer Default = new AbilityCategoryEqualityComparer();

	public bool Equals(CardLayoutElement x, CardLayoutElement y)
	{
		if (x?.card is Ability ability && y?.card is Ability ability2)
		{
			return ability.data.category == ability2.data.category;
		}
		return false;
	}

	public int GetHashCode(CardLayoutElement card)
	{
		return ((int?)(card?.card as Ability)?.data.category).GetValueOrDefault();
	}
}
