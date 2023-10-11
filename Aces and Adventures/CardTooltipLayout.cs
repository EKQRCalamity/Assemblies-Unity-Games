using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardTooltipLayout : MonoBehaviour
{
	public enum Axis
	{
		Horizontal,
		Vertical
	}

	public enum DirectionAlongAxis
	{
		Auto,
		Negative,
		Positive
	}

	public GameObject tooltipBlueprint;

	public RectTransform tooltipContainer;

	[Range(0.1f, 1f)]
	public float animationTime = 0.25f;

	public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Axis axis;

	public DirectionAlongAxis directionAlongAxis;

	public Action<Transform> onShow;

	public Action<Transform> onHide;

	private bool _isOn;

	private float _t;

	private bool _flipDirection;

	private void OnEnable()
	{
		_flipDirection = Camera.main.WorldToViewportPoint((base.transform.parent ?? base.transform).position).x > 0.5f;
		onShow?.Invoke(tooltipContainer);
	}

	private void Update()
	{
		_t += _isOn.ToFloat(1f, -1f) * Time.deltaTime / animationTime;
		_t = Mathf.Clamp01(_t);
		float num = ((directionAlongAxis == DirectionAlongAxis.Auto) ? _flipDirection.ToFloat(-1f, 1f) : ((float)((directionAlongAxis == DirectionAlongAxis.Positive) ? 1 : (-1))));
		float num2 = animationCurve.Evaluate(_t) * num;
		if (axis == Axis.Horizontal)
		{
			tooltipContainer.anchorMin = tooltipContainer.anchorMin.SetAxis(0, num2);
			tooltipContainer.anchorMax = tooltipContainer.anchorMax.SetAxis(0, num2 + 1f);
		}
		else
		{
			tooltipContainer.anchorMin = tooltipContainer.anchorMin.SetAxis(1, num);
			tooltipContainer.anchorMax = tooltipContainer.anchorMax.SetAxis(1, num);
			tooltipContainer.pivot = tooltipContainer.pivot.SetAxis(1, 1f - num2);
		}
		if (!_isOn && _t <= 0f)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (!tooltipContainer)
		{
			return;
		}
		foreach (Transform item in tooltipContainer)
		{
			item.gameObject.SetActive(value: false);
		}
		onHide?.Invoke(tooltipContainer);
	}

	public void Show(IEnumerable<string> tooltips, DirectionAlongAxis directionAlongAxisToSet = DirectionAlongAxis.Auto)
	{
		directionAlongAxis = directionAlongAxisToSet;
		using PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet(tooltips);
		using PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		foreach (CardTooltipView item in tooltipContainer.gameObject.GetComponentsInChildrenPooled<CardTooltipView>())
		{
			poolKeepItemHashSetHandle.Add(item.text.text);
		}
		foreach (string item2 in poolKeepItemHashSetHandle2.value)
		{
			if (!poolKeepItemHashSetHandle.Contains(item2))
			{
				Pools.Unpool(tooltipBlueprint, tooltipContainer).GetComponent<CardTooltipView>().SetText(item2);
			}
		}
		base.enabled = (_isOn = true);
	}

	public void Show(IEnumerable<(string text, Func<GameObject> generateIcon)> tooltips, DirectionAlongAxis directionAlongAxisToSet = DirectionAlongAxis.Auto)
	{
		directionAlongAxis = directionAlongAxisToSet;
		using PoolKeepItemHashSetHandle<(string, Func<GameObject>)> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet(tooltips);
		using PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		foreach (CardTooltipView item in tooltipContainer.gameObject.GetComponentsInChildrenPooled<CardTooltipView>())
		{
			poolKeepItemHashSetHandle.Add(item.text.text);
		}
		foreach (var item2 in poolKeepItemHashSetHandle2.value)
		{
			if (!poolKeepItemHashSetHandle.Contains(item2.Item1))
			{
				Pools.Unpool(tooltipBlueprint, tooltipContainer).GetComponent<CardTooltipView>().SetText(item2.Item1)
					.SetIcon(item2.Item2());
			}
		}
		base.enabled = (_isOn = true);
	}

	public void Hide()
	{
		_isOn = false;
	}
}
