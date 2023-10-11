using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ADeckLayoutBase : MonoBehaviour
{
	public delegate void PointerEvent(ADeckLayoutBase deckLayout, int pile, ATarget target);

	public int sortOrder;

	protected static bool IsListeningToPointerEnter => ADeckLayoutBase.OnPointerEnter != null;

	protected static bool IsListeningToPointerExit => ADeckLayoutBase.OnPointerExit != null;

	public static event PointerEvent OnClick;

	public static event PointerEvent OnPointerEnter;

	public static event PointerEvent OnPointerExit;

	protected void _SignalPointerClick(int pile, ATarget card)
	{
		ADeckLayoutBase.OnClick?.Invoke(this, pile, card);
	}

	protected void _SignalPointerEnter(int pile, ATarget card)
	{
		ADeckLayoutBase.OnPointerEnter?.Invoke(this, pile, card);
	}

	protected void _SignalPointerExit(int pile, ATarget card)
	{
		ADeckLayoutBase.OnPointerExit?.Invoke(this, pile, card);
	}

	public abstract int GetIndexOf(ATarget card);

	public abstract int GetCountInPile(ATarget card);

	public abstract void SignalPointerEnter(ATarget card);

	public abstract void SignalPointerExit(ATarget card);

	public abstract void SignalPointerClick(ATarget card, PointerEventData eventData = null);

	public abstract void SignalAtRest(ATarget card);

	public abstract void SignalSmartDrag(ACardLayout smartDragTarget, ATarget card);

	public abstract PoolKeepItemListHandle<ACardLayout> GetLayouts();

	public abstract void DestroyCard(ATarget card);

	public abstract void RepoolCard(ATarget card);

	public abstract IEnumerable<ATarget> GetNextInPiles();
}
