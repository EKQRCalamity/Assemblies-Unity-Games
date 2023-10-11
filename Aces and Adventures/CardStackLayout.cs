using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardStackLayout : ACardLayout
{
	public CardStackLayoutSettings settings;

	public ACardLayout dragLayout;

	[Range(0f, 60f)]
	[SerializeField]
	protected int _dragCount;

	public bool reverseDragLayoutOnDragEnd = true;

	private Dictionary<int, Vector3> _randomOffsets;

	private Vector3 _noiseOffset;

	private int _startIndex;

	private Dictionary<CardLayoutElement, Vector3?> _enterTargetOffsets;

	public CardLayoutRandomSettings randomness => settings.randomness;

	private Dictionary<int, Vector3> randomOffsets => _randomOffsets ?? (_randomOffsets = new Dictionary<int, Vector3>());

	private Dictionary<CardLayoutElement, Vector3?> enterTargetOffsets => _enterTargetOffsets ?? (_enterTargetOffsets = new Dictionary<CardLayoutElement, Vector3?>());

	public int dragCount
	{
		get
		{
			return _dragCount;
		}
		set
		{
			bool flag = _CanDrag(null);
			_dragCount = value;
			bool flag2 = _CanDrag(null);
			if (flag2 == flag)
			{
				return;
			}
			foreach (CardLayoutElement card in GetCards())
			{
				card.shouldShowCanDrag = flag2;
			}
		}
	}

	protected override float? _restTime => randomness.layoutRestTime;

	protected override int _startUpdateIndex => _startIndex;

	protected override bool _useDynamicTransitionTargets => settings.useDynamicTransitions;

	private void _ResetRandomOffsets()
	{
		randomOffsets.Clear();
		_noiseOffset = ACardLayout.Random.Value3() * 2048f;
	}

	private void _NudgeRandomOffset(int index, float amount)
	{
		if (randomOffsets.ContainsKey(index))
		{
			randomOffsets[index] = Vector3.Lerp(randomOffsets[index], new Vector3(ACardLayout.Random.Range(0f - randomness.position, randomness.position), ACardLayout.Random.Range(0f - randomness.position, randomness.position), ACardLayout.Random.Range(0f - randomness.rotation, randomness.rotation)), amount);
		}
	}

	private void _NudgeRandomOffsets(int index, int count)
	{
		if (settings.randomNudgeCount > 0)
		{
			for (int i = Math.Max(0, index - settings.randomNudgeCount); i <= index; i++)
			{
				_NudgeRandomOffset(i, 1f - (float)Math.Pow((float)Math.Abs(i - index) / (float)settings.randomNudgeCount, settings.randomNudgeFadePower));
			}
		}
	}

	private IEnumerator _TransferDragLayoutBackProcess()
	{
		yield return new WaitForEndOfFrame();
		_TransferDragLayoutBack();
	}

	private void _TransferDragLayoutBack()
	{
		if (!dragLayout)
		{
			return;
		}
		foreach (CardLayoutElement item in dragLayout.GetCards().AsEnumerable().ReverseIf(reverseDragLayoutOnDragEnd))
		{
			Add(item);
		}
	}

	private void Awake()
	{
		_ResetRandomOffsets();
	}

	protected override CardLayoutElement.Target _GetLayoutTarget(CardLayoutElement card, int cardIndex, int cardCount)
	{
		Vector3 position = layoutTarget.transform.position;
		Quaternion rotation = layoutTarget.transform.rotation;
		position += cardContainer.up * base.cardThickness * settings.thicknessPadding * ((float)(cardIndex + settings.thicknessPadBottom.ToInt()) + 0.5f);
		Vector3 vector3;
		if (!randomOffsets.ContainsKey(cardIndex))
		{
			Vector3 vector2 = (randomOffsets[cardIndex] = new Vector3(MathUtil.PerlinNoise(_noiseOffset.x, (float)cardIndex * randomness.positionFrequency) * randomness.position, MathUtil.PerlinNoise(_noiseOffset.y, (float)cardIndex * randomness.positionFrequency) * randomness.position, MathUtil.PerlinNoise((float)cardIndex * randomness.rotationFrequency, _noiseOffset.z) * randomness.rotation));
			vector3 = vector2;
		}
		else
		{
			vector3 = randomOffsets[cardIndex];
		}
		Vector3 vector4 = vector3;
		position += cardContainer.right * (vector4.x + (float)cardIndex * settings.leftRightSkew * base.cardWidth) + cardContainer.forward * (vector4.y + (float)cardIndex * settings.downUpSkew * base.cardHeight);
		rotation *= Quaternion.AngleAxis(vector4.z, cardContainer.up);
		return new CardLayoutElement.Target(layoutTarget, new TransformData(position, rotation, size.scaleVector));
	}

	protected override void _OnEnteredLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		_NudgeRandomOffsets(cardIndex, cardCount);
		_enterTargetOffsets?.Remove(card);
		if (cardIndex > 0)
		{
			_SetInputEnabledAt(cardIndex - 1, inputEnabled: false);
		}
	}

	protected override void _OnRemovedFromLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		base._OnRemovedFromLayout(card, cardIndex, cardCount);
		_NudgeRandomOffsets(cardIndex, cardCount);
		_enterTargetOffsets?.Remove(card);
		if (cardIndex > 0 && cardIndex == cardCount - 1)
		{
			_SetInputEnabledAt(cardIndex - 1, inputEnabled: true);
		}
	}

	public override void OnDragInitialize(PointerEventData eventData, CardLayoutElement card)
	{
		base.OnDragInitialize(eventData, card);
		if ((bool)eventData.pointerDrag && !card.isLastCardInPile)
		{
			eventData.pointerDrag = null;
		}
	}

	public override void OnDragBegin(PointerEventData eventData, CardLayoutElement card)
	{
		if (!card.isLastCardInPile)
		{
			return;
		}
		using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = Pools.UseKeepItemList(GetCards().AsEnumerable().Reverse().Take((eventData.button != 0) ? 1 : dragCount));
		foreach (CardLayoutElement item in poolKeepItemListHandle.value)
		{
			dragLayout.Add(item);
			if (item is ATargetView aTargetView)
			{
				aTargetView.frontIsVisible = true;
			}
		}
		if (GetCards().AsEnumerable().LastOrDefault() is ATargetView aTargetView2)
		{
			aTargetView2.frontIsVisible = true;
		}
		dragLayout.OnPointerEnter(eventData, card);
		dragLayout.OnPointerDown(eventData, card);
		dragLayout.OnDragInitialize(eventData, card);
		dragLayout.OnDragBegin(eventData, card);
		dragLayout.onDragEnd += _OnDragLayoutDragEnd;
	}

	private void _OnDragLayoutDragEnd(PointerEventData eventData, CardLayoutElement card)
	{
		dragLayout.onDragEnd -= _OnDragLayoutDragEnd;
		if (dragLayout.dragEndWasForced)
		{
			StartCoroutine(_TransferDragLayoutBackProcess());
		}
		else
		{
			_TransferDragLayoutBack();
		}
	}

	protected override bool _CanDrag(CardLayoutElement card)
	{
		if ((bool)dragLayout)
		{
			return dragCount > 0;
		}
		return false;
	}

	protected override void _OnAtRest()
	{
		_startIndex = int.MaxValue;
	}

	protected override void _SetAtRestDirty(CardLayoutElement card, int? cardIndex)
	{
		base._SetAtRestDirty(card, cardIndex);
		_startIndex = Math.Max(0, Math.Min(_startIndex, (cardIndex ?? card.index) - settings.randomNudgeCount));
	}

	protected override TransformData _TransformEnterLayoutTarget(CardLayoutElement card, TransformData enterLayoutTarget, int cardIndex)
	{
		if (!(settings.additionalEnterTargetThicknessPadding > 0f))
		{
			return enterLayoutTarget;
		}
		return enterLayoutTarget.Translate(enterTargetOffsets.GetValueOrDefault(card) ?? (enterTargetOffsets[card] = cardContainer.up * base.cardThickness * settings.additionalEnterTargetThicknessPadding * Math.Max(0, cardIndex - settings.randomNudgeCount - _startUpdateIndex)).Value);
	}

	private void OnValidate()
	{
		_ResetRandomOffsets();
	}
}
