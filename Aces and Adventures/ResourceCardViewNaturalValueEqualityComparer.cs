using System.Collections.Generic;

public class ResourceCardViewNaturalValueEqualityComparer : IEqualityComparer<CardLayoutElement>
{
	public static readonly ResourceCardViewNaturalValueEqualityComparer Default = new ResourceCardViewNaturalValueEqualityComparer();

	public bool Equals(CardLayoutElement x, CardLayoutElement y)
	{
		if (x != null && y != null)
		{
			return ResourceCard.NaturalValueEqualityComparer.Default.Equals(x.card as ResourceCard, y.card as ResourceCard);
		}
		return false;
	}

	public int GetHashCode(CardLayoutElement obj)
	{
		if (!(obj.card is ResourceCard card))
		{
			return 0;
		}
		return ResourceCard.NaturalValueEqualityComparer.Default.GetHashCode(card);
	}
}
