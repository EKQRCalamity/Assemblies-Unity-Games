using System;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHandLayout : ACardLayout
{
	public enum Alignment
	{
		Horizontal,
		Vertical
	}

	public Target dragTarget;

	public Transform alternateDragTransform;

	public CardLayoutSpringSettings fullyDraggingSpringSettings;

	public CardHandLayoutSettings settings;

	public CardLayoutRandomSettings randomness;

	public Alignment alignment;

	public bool checkForNestedLayouts;

	public int spacingCardCountOffset;

	public bool dragEntireHand;

	public Target dragStayTarget;

	private TransformSpringSettings _lerpedSpringSettings;

	private CardLayoutElement _pointerOverAnticipation;

	private Vector3 _dragEntireHandOffset;

	private List<Vector2> _realisticRotations;

	private AxisType _spacingAxis
	{
		get
		{
			if (alignment != 0)
			{
				return AxisType.Z;
			}
			return AxisType.X;
		}
	}

	private AxisType _orthogonalAxis
	{
		get
		{
			if (alignment != 0)
			{
				return AxisType.X;
			}
			return AxisType.Z;
		}
	}

	private float _spacingAxisSign => (alignment == Alignment.Horizontal) ? 1 : (-1);

	private TransformSpringSettings lerpedSpringSettings => _lerpedSpringSettings ?? (_lerpedSpringSettings = new TransformSpringSettings());

	private List<Vector2> realisticRotations => _realisticRotations ?? (_realisticRotations = new List<Vector2>());

	public override CardLayoutElement.PointerOverPadding? pointerOverPaddingOverride { get; protected set; }

	protected override CardLayoutElement.PointerOverPadding _pointerOverPadding => settings.pointerOverPadding;

	protected override bool _addSorted => settings.useUnderlyingOrder;

	protected override Transform _dragTarget
	{
		get
		{
			if (!alternateDragTransform || base.inputButton != PointerEventData.InputButton.Right)
			{
				if (!dragTarget.transform)
				{
					return base._dragTarget;
				}
				return dragTarget.transform;
			}
			return alternateDragTransform;
		}
	}

	protected override float _dragLerp
	{
		get
		{
			if (settings.dragPlaneLerpDistance == 0f)
			{
				return 1f;
			}
			float num = size[_orthogonalAxis] * settings.dragPlaneLerpDistance;
			return Mathf.Clamp01((Mathf.Abs(Vector3.Dot(_dragTargetPosition - layoutTarget.transform.position, _dragTarget.GetAxis(_orthogonalAxis))) - num) / num.InsureNonZero());
		}
	}

	public override bool draggingEntireHand
	{
		get
		{
			if ((bool)_pointerDrag && dragEntireHand)
			{
				return base.inputButton == PointerEventData.InputButton.Left;
			}
			return false;
		}
	}

	protected override bool _useDragOffset => settings.useDragOffset;

	protected override bool _processExitTarget => settings.processExitTargets;

	protected override bool _checkForNestedLayouts => checkForNestedLayouts;

	protected override bool _useDynamicTransitionTargets
	{
		get
		{
			if (!settings.useDynamicTransitionTargets)
			{
				return settings.useRealisticRotation;
			}
			return true;
		}
	}

	public override CardLayoutElement.PointerOverPadding defaultPointerOverPadding => settings.defaultColliderPadding;

	protected override float? _restTime => randomness.layoutRestTime;

	private float _Spacing(int cardCount)
	{
		float num = Mathf.Lerp(settings.spacingAtMinCardCount, settings.spacingAtMaxCardCount, (float)(cardCount + spacingCardCountOffset) / (float)settings.maxCardCount);
		if (settings.maxTotalSpacing > 0f && cardCount > 1)
		{
			num = Math.Min(num, base.usesCardGrouping ? (num * (settings.maxTotalSpacing / (_TotalSpacing(num, cardCount) * size.scale + (size.scale - 1f) * 2f))) : (Math.Max(0f, settings.maxTotalSpacing * size.inverseScale - 1f) / (float)(cardCount - 1)));
		}
		return num;
	}

	private float _Spacing(int cardCount, int cardIndex)
	{
		return _Spacing(cardCount) * ((base.usesCardGrouping && _BelongsToGroupButNotFirstInGroup(cardIndex)) ? settings.groupSpacing : 1f);
	}

	private float _TotalSpacing(float spacing, int cardCount)
	{
		return ((float)cardCount - (float)_GetGroupingCountUpToIndex(cardCount - 1) * (1f - settings.groupSpacing)) * spacing;
	}

	private void _UpdatePointerOverAnticipationCard()
	{
		if (settings.pointerOverAnticipateDistance <= 0f)
		{
			return;
		}
		PoolKeepItemListHandle<CardLayoutElement> cards = GetCards();
		try
		{
			CardLayoutElement pointerOverAnticipation = null;
			Ray mouseRay;
			float subtractScalar;
			float addScalar;
			if (base.inputIndex < 0 && cards.Count > 1)
			{
				mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				subtractScalar = _spacingAxisSign * (0f - size[_spacingAxis]) * settings.reverseLayoutOrder.ToFloat(-1f, 1f);
				addScalar = settings.pointerOverAnticipateDistance * size[_orthogonalAxis];
				if (!settings.reverseLayoutOrder)
				{
					int num = cards.value.Count - 1;
					while (num >= 0 && !RayCastPointerOverAnticipation(cards[num], num))
					{
						num--;
					}
				}
				else
				{
					for (int i = 0; i < cards.value.Count && !RayCastPointerOverAnticipation(cards[i], i); i++)
					{
					}
				}
			}
			if (!SetPropertyUtility.SetObject(ref _pointerOverAnticipation, pointerOverAnticipation))
			{
				return;
			}
			foreach (CardLayoutElement item in cards.value)
			{
				item.pointerOverPadding = defaultPointerOverPadding.PadAxis(_orthogonalAxis, ((bool)pointerOverAnticipation && item != pointerOverAnticipation) ? settings.pointerOverAnticipateShrinkAmount : 0f);
			}
			bool RayCastPointerOverAnticipation(CardLayoutElement card, int cardIndex)
			{
				Transform transform = card.inputCollider.transform;
				Vector3 v = transform.GetAxis(_spacingAxis) * subtractScalar * Math.Abs(_Spacing(cards.Count, cardIndex));
				Vector3 vector = transform.GetAxis(_orthogonalAxis) * addScalar;
				if (!new Rect3D(transform.position, transform.up, transform.forward, new Vector2(base.cardWidth, base.cardHeight)).SubtractSizeAlongVector(v).AddSizeAlongVector(vector).AddSizeAlongVector(-vector)
					.Raycast(mouseRay)
					.HasValue)
				{
					return false;
				}
				return pointerOverAnticipation = card;
			}
		}
		finally
		{
			if (cards != null)
			{
				((IDisposable)cards).Dispose();
			}
		}
	}

	private void _UpdateRealisticRotations()
	{
		if (settings.useRealisticRotation)
		{
			realisticRotations.Clear();
			int count = base.Count;
			float num = 1f / size[_spacingAxis];
			float num2 = base.cardThickness;
			float num3 = 0f;
			float b = 0f;
			realisticRotations.Add(Vector2.zero);
			for (int i = 1; i < count; i++)
			{
				float num4 = Mathf.Clamp(Mathf.Abs(_Spacing(count, i)), 0.001f, 1f);
				float num5 = (float)i * num2;
				float num6 = Mathf.Lerp(num3 + num2, 0f, num4);
				float num7 = Mathf.Lerp(num3, b, 1f - num4) + ((num4 < 1f) ? num2 : 0f);
				float x = (num6 + num7) * 0.5f - num5;
				realisticRotations.Add(new Vector2(x, Mathf.Asin((num7 - num6) * num) * 57.29578f));
				num3 = num6;
				b = num7;
			}
		}
	}

	public override void OnDragBegin(PointerEventData eventData, CardLayoutElement card)
	{
		base.OnDragBegin(eventData, card);
		if (dragEntireHand && base.inputButton == PointerEventData.InputButton.Left)
		{
			_dragEntireHandOffset = (settings.centerDragEntireHand ? Vector3.zero : (layoutTarget.transform.position - eventData.pointerPressRaycast.worldPosition).ProjectOntoPlane(layoutTarget.transform.up));
			if (GetCards().AsEnumerable().AtLeast(2))
			{
				_pointerDrag?.GetComponentInChildren<DragTooltipVisibility>()?.Stop();
			}
		}
	}

	public override void OnDragEnd(PointerEventData eventData, CardLayoutElement card)
	{
		base.OnDragEnd(eventData, card);
		if (settings.doEnterTransitionsOnEndDragStay == CardHandLayoutSettings.EnterTransitionOnEndDrag.Never || !(card.GetComponentInParent<ACardLayout>() == this) || dragEntireHand)
		{
			return;
		}
		bool flag = false;
		switch (settings.doEnterTransitionsOnEndDragStay)
		{
		case CardHandLayoutSettings.EnterTransitionOnEndDrag.Always:
			flag = true;
			break;
		case CardHandLayoutSettings.EnterTransitionOnEndDrag.OnDraggedWithinThreshold:
			flag = !base._draggedBeyondThreshold;
			break;
		case CardHandLayoutSettings.EnterTransitionOnEndDrag.OnDraggedBeyondThreshold:
			flag = base._draggedBeyondThreshold;
			break;
		}
		if (flag)
		{
			foreach (CardLayoutElement.Target item in _GetEnterTargets(card))
			{
				card.targets.Add(item);
			}
			if (card.targets.Count > 0)
			{
				_SetInputEnabled(card, inputEnabled: false);
				_SetAtRestDirty(card, null);
			}
		}
		else if ((bool)dragStayTarget?.transform)
		{
			using (PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = GetCards())
			{
				card.targets.Add(new CardLayoutElement.Target(dragStayTarget, _GetLayoutTarget(card, poolKeepItemListHandle.value.IndexOf(card), poolKeepItemListHandle.Count).target.Transform(dragStayTarget.transform.localToWorldMatrix * layoutTarget.transform.worldToLocalMatrix)));
			}
		}
	}

	protected override void Update()
	{
		if (!base._isAtRest)
		{
			_UpdateRealisticRotations();
		}
		base.Update();
		_UpdatePointerOverAnticipationCard();
	}

	protected override CardLayoutElement.Target _GetLayoutTarget(CardLayoutElement card, int cardIndex, int cardCount)
	{
		Vector3 position = layoutTarget.transform.position;
		int num = cardCount - 1;
		float num2 = (float)(settings.reverseLayoutOrder ? (num - cardIndex) : cardIndex) - (float)num * 0.5f;
		float num3 = base.cardThickness * settings.thicknessPadding;
		position += layoutTarget.transform.up * (num3 * (num2 + (float)cardCount * (0.5f - settings.thicknessCenter)));
		float num4 = _Spacing(cardCount);
		float num5 = size[_spacingAxis];
		float num6 = num5 * num4;
		Vector3 vector = layoutTarget.transform.GetAxis(_spacingAxis) * _spacingAxisSign;
		float num7 = settings.alignmentCenter;
		if (settings.alignmentCenterAtMaxTotalSpacing >= 0f && settings.maxTotalSpacing > 0f)
		{
			num7 = Mathf.Lerp(num7, settings.alignmentCenterAtMaxTotalSpacing, Mathf.Pow(Math.Max(0f, MathUtil.GetLerpAmount(settings.spacingAtMaxCardCount, settings.targetSpacingForAlignmentCenterCorrection, num4)), settings.alignmentCenterCorrectionPower));
		}
		position += vector * num6 * (num2 + (float)num * (num7 - 0.5f));
		if (base.usesCardGrouping)
		{
			position += vector * _GetGroupingCountUpToIndex(cardIndex) * (settings.groupSpacing - 1f) * num4 * num5;
			position += vector * _GetGroupingCountUpToIndex(num) * (1f - settings.groupSpacing) * num5 * num4 * (1f - num7);
		}
		float num8 = settings.fanRotationDegrees / (float)cardCount * (MathF.PI / 180f);
		float num9 = num6 / num8.InsureNonZero();
		float num10 = num8 * num2;
		Quaternion quaternion = layoutTarget.transform.rotation;
		Vector3 axis = layoutTarget.transform.GetAxis(_orthogonalAxis);
		if (card != base._effectivePointerOver || (bool)_pointerDrag || settings.applyFanningToPointerOver)
		{
			position += axis * num9 * (Mathf.Cos(num10) - 1f);
			quaternion = Quaternion.AngleAxis(num10 * 57.29578f, layoutTarget.transform.up) * quaternion;
			if ((!randomness.onlyApplyToCardsWithActiveOffset || card.hasOffset) && (!randomness.disableWhenInputActive || base.inputIndex < 0) && card.noiseAnimationEnabled)
			{
				float num11 = (float)((randomness.sameForAllInLayout ? GetInstanceID() : card.GetInstanceID()) % 10000) * 0.5f;
				if (randomness.rotation > 0f)
				{
					float num12 = Time.time * randomness.rotationFrequency + num11 * 0.5f;
					quaternion = ((!randomness.applyToAllAxes) ? (Quaternion.AngleAxis(MathUtil.PerlinNoise(num11, num11 + num12) * randomness.rotation, layoutTarget.transform.up) * quaternion) : (Quaternion.Euler(new Vector3(MathUtil.PerlinNoise(num11, num11 + num12), MathUtil.PerlinNoise(num11 + num12, num11), MathUtil.PerlinNoise(num12 * MathUtil.OneOverSqrtTwo - num11, num12 * (0f - MathUtil.OneOverSqrtTwo) - num11)) * randomness.rotation) * quaternion));
				}
				if (randomness.position > 0f)
				{
					float num13 = Time.time * randomness.positionFrequency;
					position += layoutTarget.transform.forward * MathUtil.PerlinNoise(num11, num11 + num13) * randomness.position;
					position += layoutTarget.transform.right * MathUtil.PerlinNoise(num11 + num13, num11) * randomness.position;
					if (randomness.applyToAllAxes)
					{
						position += layoutTarget.transform.up * MathUtil.PerlinNoise(num13 * MathUtil.OneOverSqrtTwo - num11, num13 * (0f - MathUtil.OneOverSqrtTwo) - num11) * randomness.position;
					}
				}
			}
		}
		if (base.inputIndex >= 0)
		{
			bool flag = dragEntireHand && (bool)_pointerDrag && base.inputButton == PointerEventData.InputButton.Left;
			float num14 = Math.Abs(num4);
			float num15 = 1f - num14;
			if (cardIndex < base.inputIndex && !flag)
			{
				float num16 = (settings.reverseLayoutOrder ? (0f - settings.pointerOverSpacingRight) : settings.pointerOverSpacingLeft);
				if (Math.Abs(num16) > num14 || !settings.pointerOverSpacingOnlyIfGreater)
				{
					position -= vector * num15 * num16 * num5;
				}
				if (settings.pointerOverUpEntireHand && settings.reverseLayoutOrder)
				{
					position += layoutTarget.transform.up * settings.pointerOverUp * size.scaledAverage;
				}
			}
			else if (cardIndex > base.inputIndex && !flag)
			{
				float num17 = (settings.reverseLayoutOrder ? (0f - settings.pointerOverSpacingLeft) : settings.pointerOverSpacingRight);
				if (Math.Abs(num17) > num14 || !settings.pointerOverSpacingOnlyIfGreater)
				{
					position += vector * num15 * num17 * num5;
				}
				if (settings.pointerOverUpEntireHand && !settings.reverseLayoutOrder)
				{
					position += layoutTarget.transform.up * settings.pointerOverUp * size.scaledAverage;
				}
			}
			else
			{
				float num18 = size[_orthogonalAxis];
				if ((bool)dragTarget.transform && (card == _pointerDrag || flag))
				{
					float num19 = _dragLerp * settings.dragRotationPerSpeed;
					if (!flag)
					{
						quaternion = Quaternion.AngleAxis((0f - Vector3.Dot(_dragTarget.right, card.velocities.position)) * num19, _dragTarget.forward) * Quaternion.AngleAxis(Vector3.Dot(_dragTarget.forward, card.velocities.position) * num19, _dragTarget.right) * layoutTarget.transform.rotation.SlerpUnclamped(_dragTarget.rotation, _dragLerp);
					}
					return new CardLayoutElement.Target(dragTarget, new TransformData(flag ? (position + (_dragTargetPosition - layoutTarget.transform.position) + _dragEntireHandOffset) : (_dragTargetPosition + layoutTarget.transform.up * Math.Max(num3, base.cardThickness) * ((float)cardCount + settings.dragThicknessPadding) * 0.5f * (_dragTarget != layoutTarget.transform).ToFloat(_dragLerp.OneMinusIf(), 1f)), quaternion, size.scaleVector.Max(dragTarget.transform.localScale)));
				}
				position += vector * settings.pointerOverSpacingSelf * num5;
				float num20 = (settings.correctPointerOverRaiseForScale ? (0.5f + (settings.pointerOverRaise - 0.5f) * size.inverseScale) : settings.pointerOverRaise);
				position += axis * num20 * num18;
				position += vector * Vector3.Dot(vector, (Quaternion.AngleAxis(num10 * 57.29578f, layoutTarget.transform.up) * quaternion).GetAxis(_orthogonalAxis)) * num18 * num20;
				position += layoutTarget.transform.up * settings.pointerOverUp * size.scaledAverage;
				CardLayoutElement.PointerOverPadding pointerOverPadding = pointerOverPaddingOverride ?? settings.pointerOverPadding;
				if (cardIndex == 0)
				{
					pointerOverPadding.xPadding.x += settings.pointerOverEdgePadding;
				}
				else if (cardIndex == num)
				{
					pointerOverPadding.xPadding.y += settings.pointerOverEdgePadding;
				}
				if ((bool)pointerOverPadding)
				{
					pointerOverPadding.zPadding *= size[AxisType.X] / size[AxisType.Z];
					card.pointerOverPadding = pointerOverPadding;
				}
			}
		}
		CardLayoutElement.Target result = new CardLayoutElement.Target(layoutTarget, new TransformData(position, quaternion, size.scaleVector));
		if (settings.useRealisticRotation && cardIndex < realisticRotations.Count)
		{
			return result.SetTarget(result.target.Translate(layoutTarget.transform.up * realisticRotations[cardIndex].x).Rotate(Quaternion.AngleAxis(realisticRotations[cardIndex].y, (alignment == Alignment.Horizontal) ? Vector3.back : Vector3.left)));
		}
		return result;
	}

	protected override TransformSpringSettings _GetSpringSettings(CardLayoutElement.Target target)
	{
		if ((bool)_pointerDrag && (bool)fullyDraggingSpringSettings)
		{
			return lerpedSpringSettings.SetAsLerpBetween(base._GetSpringSettings(target), fullyDraggingSpringSettings, _dragLerp);
		}
		return base._GetSpringSettings(target);
	}

	protected override TransformData _ProcessExitTarget(CardLayoutElement card, ref TransformData cardTransformData, bool wasPointerOver, List<CardLayoutElement> cards)
	{
		if (!settings.adjustExitTargetsForPointerOver || !wasPointerOver)
		{
			return cardTransformData;
		}
		CardHandLayoutSettings.PointerOverSettings pointerOverSettings = settings.GetPointerOverSettings();
		settings.SetPointerOverSettings(CardHandLayoutSettings.PointerOverSettings.Default);
		CardLayoutElement.Target target = _GetLayoutTarget(card, cards.IndexOf(card), cards.Count);
		settings.SetPointerOverSettings(pointerOverSettings);
		return target;
	}

	protected override bool _ProcessEnterTarget(Target target, int cardIndex, int cardCount, int enterTargetIndex)
	{
		if (settings.spacingBasedEnterTransition == 0f)
		{
			return true;
		}
		bool flag = settings.spacingBasedEnterType switch
		{
			CardHandLayoutSettings.SpacingBasedEnterTransitionType.AllButEndCard => cardIndex != ((!settings.reverseLayoutOrder) ? (cardCount - 1) : 0), 
			CardHandLayoutSettings.SpacingBasedEnterTransitionType.WhenNotEmpty => cardCount > 1, 
			CardHandLayoutSettings.SpacingBasedEnterTransitionType.Always => true, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		float value = target.transform.localPosition[1];
		target.transform.localPosition = (target.transform.localPosition.normalized * settings.spacingBasedEnterTransition * size[_spacingAxis] * Mathf.Max(0.01f, flag ? (1f - _Spacing(cardCount, cardIndex)) : 0f)).SetAxis(AxisType.Y, value);
		if (!flag)
		{
			return enterTargetIndex == enterTargets.Count - 1;
		}
		return true;
	}

	protected override bool _CanDrag(CardLayoutElement card)
	{
		if (settings.canDrag == CardHandLayoutSettings.CanDragWhen.Always || (settings.canDrag == CardHandLayoutSettings.CanDragWhen.WhenNotOffset && !card.hasOffset))
		{
			return dragTarget.springSettings;
		}
		return false;
	}

	protected override bool _ShouldPlayPointerOverAudio(CardLayoutElement card)
	{
		if (settings.playPointerOverSoundOnFirstCard && settings.playPointerOverSoundOnLastCard)
		{
			return true;
		}
		using PoolKeepItemListHandle<CardLayoutElement> poolKeepItemListHandle = GetCards();
		if (!settings.playPointerOverSoundOnFirstCard && card == poolKeepItemListHandle[0])
		{
			return false;
		}
		if (!settings.playPointerOverSoundOnLastCard && card == poolKeepItemListHandle[poolKeepItemListHandle.Count - 1])
		{
			return false;
		}
		return true;
	}

	protected override void _OnAddedToLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		base._OnAddedToLayout(card, cardIndex, cardCount);
		if (settings.onlyTopCardHasInputEnabled && cardIndex > 0)
		{
			_SetInputEnabledAt(cardIndex - 1, inputEnabled: false);
		}
		if (!settings.inputEnabled)
		{
			_SetInputEnabled(card, inputEnabled: false);
		}
	}

	protected override void _OnEnteredLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		base._OnEnteredLayout(card, cardIndex, cardCount);
		if (settings.onlyTopCardHasInputEnabled && cardIndex < cardCount - 1)
		{
			_SetInputEnabled(card, inputEnabled: false);
		}
		if (!settings.inputEnabled)
		{
			_SetInputEnabled(card, inputEnabled: false);
		}
	}

	protected override void _OnRemovedFromLayout(CardLayoutElement card, int cardIndex, int cardCount)
	{
		base._OnRemovedFromLayout(card, cardIndex, cardCount);
		if (settings.onlyTopCardHasInputEnabled && cardIndex > 0 && cardIndex == cardCount - 1)
		{
			_SetInputEnabledAt(cardIndex - 1, inputEnabled: true);
		}
		if (!settings.inputEnabled)
		{
			_SetInputEnabled(card, inputEnabled: true);
		}
	}
}
