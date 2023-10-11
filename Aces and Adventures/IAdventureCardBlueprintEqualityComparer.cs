using System.Collections.Generic;

public class IAdventureCardBlueprintEqualityComparer : IEqualityComparer<IAdventureCard>
{
	public static readonly IAdventureCardBlueprintEqualityComparer Default = new IAdventureCardBlueprintEqualityComparer();

	public bool Equals(IAdventureCard x, IAdventureCard y)
	{
		return x?.blueprint?.path == y?.blueprint?.path;
	}

	public int GetHashCode(IAdventureCard obj)
	{
		if (obj.blueprint == null)
		{
			return 0;
		}
		return obj.blueprint.path.GetHashCode();
	}
}
