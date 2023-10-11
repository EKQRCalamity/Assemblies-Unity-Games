using System.Collections.Generic;

public abstract class ACardLayoutGroup : ACardLayout
{
	protected abstract void _OnCardAddedToLayout(ACardLayout layout, CardLayoutElement card);

	protected abstract void _OnCardRemovedFromLayout(ACardLayout layout, CardLayoutElement card);

	protected abstract ACardLayout _GetAddLayout(CardLayoutElement card);

	public abstract PoolKeepItemListHandle<ACardLayout> GetLayouts();

	public abstract ACardLayout GetLayout(CardLayoutElement card);

	protected virtual void Awake()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.onCardAdded += _OnCardAddedToLayout;
			layout.onCardRemoved += _OnCardRemovedFromLayout;
		}
	}

	public override CardLayoutElement Add(CardLayoutElement card, bool addEvenIfInLayoutAlready = false)
	{
		return _GetAddLayout(card).Add(card, addEvenIfInLayoutAlready);
	}

	protected override void Update()
	{
	}

	protected override CardLayoutElement.Target _GetLayoutTarget(CardLayoutElement card, int cardIndex, int cardCount)
	{
		return default(CardLayoutElement.Target);
	}

	public override void RefreshPointerOver()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.RefreshPointerOver();
		}
	}

	public override void RefreshPointerExit()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.RefreshPointerExit();
		}
	}

	public override PoolKeepItemListHandle<CardLayoutElement> GetCards()
	{
		PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = Pools.UseKeepItemList<CardLayoutElement>();
		foreach (ACardLayout layout in GetLayouts())
		{
			foreach (CardLayoutElement card in layout.GetCards())
			{
				poolKeepItemListHandle.Add(card);
			}
		}
		return poolKeepItemListHandle;
	}

	public override void ClearSmartTargets()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.ClearSmartTargets();
		}
	}

	public override TransformData GetLayoutTarget(CardLayoutElement card)
	{
		return GetLayout(card).GetLayoutTarget(card);
	}

	public override bool IsAtRest()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			if (!layout.IsAtRest())
			{
				return false;
			}
		}
		return true;
	}

	public override DragThresholdData SetDragThresholds(DragThresholdData dragThresholds)
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.SetDragThresholds(dragThresholds);
		}
		return base.SetDragThresholds(dragThresholds);
	}

	public override void ForceFinishLayoutAnimations()
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.ForceFinishLayoutAnimations();
		}
	}

	public override void RemoveCard(CardLayoutElement card)
	{
		foreach (ACardLayout layout in GetLayouts())
		{
			layout.RemoveCard(card);
		}
	}

	public override IEnumerable<ACardLayout> GetCardLayouts()
	{
		return GetLayouts().AsEnumerable();
	}
}
