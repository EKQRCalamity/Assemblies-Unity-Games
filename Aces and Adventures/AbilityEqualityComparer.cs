using System.Collections.Generic;

public class AbilityEqualityComparer : IEqualityComparer<Ability>
{
	public static readonly AbilityEqualityComparer Default = new AbilityEqualityComparer();

	public bool Equals(Ability x, Ability y)
	{
		return ContentRef.Equal(x?.dataRef, y?.dataRef);
	}

	public int GetHashCode(Ability card)
	{
		return card?.dataRef.GetHashCode() ?? 0;
	}
}
