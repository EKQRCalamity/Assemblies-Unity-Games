using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutOffset
{
	private PoolKeepItemDictionaryHandle<CardLayoutElement, Matrix4x4> _offsetCards;

	public static event Action OnClearLayoutOffsetsDelayed;

	public LayoutOffset()
	{
	}

	public LayoutOffset(IEnumerable<ACardLayout> layoutsToOffset, Plane minPlane)
	{
		OffsetLayouts(layoutsToOffset, minPlane);
	}

	public void OffsetLayouts(IEnumerable<ACardLayout> layoutsToOffset, Plane minPlane)
	{
		_offsetCards = Pools.UseKeepItemDictionary<CardLayoutElement, Matrix4x4>();
		foreach (ACardLayout item in layoutsToOffset)
		{
			using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = item.GetCards();
			float num = minPlane.GetDistanceToPoint(item.layoutTarget.transform.position);
			foreach (CardLayoutElement item2 in poolKeepItemListHandle.value)
			{
				num = Mathf.Min(num, minPlane.GetDistanceToPoint(item.GetLayoutTargetWithOffset(item2).position));
			}
			if (num >= 0f)
			{
				continue;
			}
			Matrix4x4 matrix4x = Matrix4x4.Translate(minPlane.normal * (0f - num + 0.02f));
			foreach (CardLayoutElement item3 in poolKeepItemListHandle.value)
			{
				if (_offsetCards.value.TryAdd(item3, matrix4x))
				{
					item3.offsets.Insert(0, matrix4x);
				}
			}
		}
	}

	public void ClearLayoutOffsets()
	{
		if (!_offsetCards)
		{
			return;
		}
		foreach (KeyValuePair<CardLayoutElement, Matrix4x4> offsetCard in _offsetCards)
		{
			offsetCard.Key.offsets.Remove(offsetCard.Value);
		}
	}

	public IEnumerator ClearLayoutOffsetsDelayed(float delay = 0.4f)
	{
		while (true)
		{
			float num;
			delay = (num = delay - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			yield return null;
		}
		if ((bool)_offsetCards)
		{
			foreach (KeyValuePair<CardLayoutElement, Matrix4x4> offsetCard in _offsetCards)
			{
				if ((bool)offsetCard.Key)
				{
					offsetCard.Key.offsets.Remove(offsetCard.Value);
				}
			}
		}
		LayoutOffset.OnClearLayoutOffsetsDelayed?.Invoke();
	}

	public void ClearOffset(CardLayoutElement card)
	{
		if ((bool)_offsetCards && _offsetCards.ContainsKey(card))
		{
			Matrix4x4 item = _offsetCards[card];
			if (_offsetCards.Remove(card))
			{
				card.offsets.Remove(item);
			}
		}
	}
}
