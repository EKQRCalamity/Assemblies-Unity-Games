using System.Collections.Generic;

public class ResourceCardViewValueEqualityComparer : IEqualityComparer<CardLayoutElement>
{
	public static readonly ResourceCardViewValueEqualityComparer Default = new ResourceCardViewValueEqualityComparer();

	public bool Equals(CardLayoutElement x, CardLayoutElement y)
	{
		if (x?.card is ResourceCard resourceCard && y?.card is ResourceCard resourceCard2)
		{
			return resourceCard.value == resourceCard2.value;
		}
		return false;
	}

	public int GetHashCode(CardLayoutElement obj)
	{
		return ((int?)(obj?.card as ResourceCard)?.value).GetValueOrDefault();
	}
}
