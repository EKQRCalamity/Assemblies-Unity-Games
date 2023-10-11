using System.Collections.Generic;
using UnityEngine;

public class CardSlottedLayoutGroup : ACardLayoutGroup
{
	[Header("Slots===============================================================================================================")]
	public ACardLayout[] layouts;

	public ACardLayout overflowLayout;

	[Range(1f, 10f)]
	public int countPerLayout = 1;

	private Dictionary<CardLayoutElement, ACardLayout> _layoutMap = new Dictionary<CardLayoutElement, ACardLayout>();

	public override int maxCount => layouts.Length * countPerLayout;

	protected override void _UpdateCardGrouping()
	{
		_layoutMap = new Dictionary<CardLayoutElement, ACardLayout>(base.groupingEqualityComparer);
		foreach (ACardLayout layout in GetLayouts())
		{
			foreach (CardLayoutElement card in layout.GetCards())
			{
				_layoutMap[card] = layout;
			}
		}
	}

	protected override void _OnCardAddedToLayout(ACardLayout layout, CardLayoutElement card)
	{
		_layoutMap[card] = layout;
	}

	protected override void _OnCardRemovedFromLayout(ACardLayout layout, CardLayoutElement card)
	{
		if (layout.Count == 0)
		{
			_layoutMap.Remove(card);
		}
	}

	protected override ACardLayout _GetAddLayout(CardLayoutElement card)
	{
		ACardLayout valueOrDefault = _layoutMap.GetValueOrDefault(card);
		if ((object)valueOrDefault != null)
		{
			return valueOrDefault;
		}
		ACardLayout[] array = layouts;
		foreach (ACardLayout aCardLayout in array)
		{
			if (aCardLayout.Count < countPerLayout)
			{
				return aCardLayout;
			}
		}
		return overflowLayout;
	}

	public override PoolKeepItemListHandle<ACardLayout> GetLayouts()
	{
		PoolKeepItemListHandle<ACardLayout> poolKeepItemListHandle = Pools.UseKeepItemList<ACardLayout>();
		ACardLayout[] array = layouts;
		foreach (ACardLayout aCardLayout in array)
		{
			if ((bool)aCardLayout)
			{
				poolKeepItemListHandle.Add(aCardLayout);
			}
		}
		if ((bool)overflowLayout)
		{
			poolKeepItemListHandle.Add(overflowLayout);
		}
		return poolKeepItemListHandle;
	}

	public override ACardLayout GetLayout(CardLayoutElement card)
	{
		return _layoutMap[card];
	}
}
