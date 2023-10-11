using System.Collections.Generic;

public class AbilityDataRefEqualityComparer : IEqualityComparer<CardLayoutElement>
{
	public static readonly AbilityDataRefEqualityComparer Default = new AbilityDataRefEqualityComparer();

	public bool Equals(CardLayoutElement x, CardLayoutElement y)
	{
		if (x?.card is Ability ability && y?.card is Ability ability2)
		{
			return ContentRef.Equal(ability.dataRef, ability2.dataRef);
		}
		return false;
	}

	public int GetHashCode(CardLayoutElement card)
	{
		return (card?.card as Ability)?.dataRef.GetHashCode() ?? 0;
	}
}
