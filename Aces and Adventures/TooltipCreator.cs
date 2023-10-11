using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TooltipCreator : MonoBehaviour
{
	private const float CANVAS_EDGE_PADDING = 12f;

	private static readonly TextBuilder _Builder = new TextBuilder(clearOnToString: true);

	private static readonly ResourceBlueprint<GameObject> TooltipBlueprint = "UI/Tooltips/Fitter";

	public static readonly ResourceBlueprint<GameObject> TooltipTextBlueprint = "UI/Tooltips/Text";

	private static readonly ResourceBlueprint<GameObject> RectSyncBlueprint = "UI/Tooltips/RectTransformSync";

	public float padding;

	public float canvasEdgePadding = 12f;

	public TooltipDirection direction;

	public TooltipOrthogonalDirection orthogonalDirection = TooltipOrthogonalDirection.Center;

	public float contentScale = 1f;

	public bool matchContentScaleWithCreator;

	public bool deactivateContentOnHide = true;

	public bool recurseRect;

	public bool trackCreator;

	public bool blocksRayCasts;

	public bool useOppositeDirection;

	private TooltipFitter _tooltip;

	private HashSet<TooltipGenerator> _generators;

	private sbyte _forcedMainAxisPivot = -1;

	public int forcedMainAxisPivot
	{
		get
		{
			return _forcedMainAxisPivot;
		}
		set
		{
			_forcedMainAxisPivot = (sbyte)Math.Sign(value);
		}
	}

	private static void _CreateCommon(TooltipGenerator tooltip, bool beginShowTimer, float waitTime, float padding, float canvasEdgePadding, TooltipDirection direction, TooltipOrthogonalDirection orthogonalDirection, float contentScale, bool matchContentScaleWithCreator, bool deactivateContentOnHide, bool recurseRect, bool trackCreator, bool ignoreEventsWhileDragging, bool ignorePointerEvents, bool blockRayCasts, bool ignoreClear, bool useOppositeDirection, bool useProxyTooltipCreator, bool clearOnDisable, int? forcedMainAxisPivot)
	{
		TooltipVisibility tooltipVisibility = tooltip.visibility;
		tooltipVisibility.waitTime = Mathf.Max(0f, waitTime);
		tooltipVisibility.EndTimer();
		if (beginShowTimer)
		{
			tooltipVisibility.StartTimer();
		}
		TooltipCreator creator = tooltip.creator;
		creator.padding = padding;
		creator.canvasEdgePadding = canvasEdgePadding;
		creator.direction = direction;
		creator.orthogonalDirection = orthogonalDirection;
		creator.contentScale = contentScale;
		creator.matchContentScaleWithCreator = matchContentScaleWithCreator;
		creator.deactivateContentOnHide = deactivateContentOnHide;
		creator.recurseRect = recurseRect;
		creator.trackCreator = trackCreator;
		creator.blocksRayCasts = blockRayCasts;
		creator.useOppositeDirection = useOppositeDirection;
		if (forcedMainAxisPivot.HasValue)
		{
			creator.forcedMainAxisPivot = forcedMainAxisPivot.Value;
		}
		tooltip.ignorePointerEventsWhileDragging = ignoreEventsWhileDragging;
		tooltip.ignorePointerEvents = ignorePointerEvents;
		tooltip.ignoreClearTooltipRequests = ignoreClear;
		tooltip.clearOnDisable = clearOnDisable;
		creator.AddGenerator(tooltip);
		foreach (TooltipGenerator generator in creator._generators)
		{
			generator.enabled = generator.GetType() == tooltip.GetType();
		}
		if (useProxyTooltipCreator)
		{
			tooltipVisibility.OnHideTooltip.AddListener(OnHide);
		}
		void OnHide()
		{
			tooltip.gameObject.SetActive(value: false);
			tooltipVisibility.OnHideTooltip.RemoveListener(OnHide);
		}
	}

	private static Transform _GetTooltipCreator(Transform tooltipCreator, bool useProxyTooltipCreator)
	{
		if (useProxyTooltipCreator)
		{
			return Pools.Unpool(RectSyncBlueprint, tooltipCreator.GetComponentInParent<Canvas>().transform).GetComponent<RectTransformSync>().SetData(tooltipCreator as RectTransform)
				.transform;
		}
		return tooltipCreator;
	}

	public static Transform CreateTextTooltip(Transform tooltipCreator, string text, bool beginShowTimer = true, float waitTime = 0.2f, bool backgroundEnabled = true, TextAlignmentOptions alignment = TextAlignmentOptions.Center, float padding = 12f, float canvasEdgePadding = 12f, TooltipDirection direction = TooltipDirection.Horizontal, TooltipOrthogonalDirection orthogonalDirection = TooltipOrthogonalDirection.Center, float contentScale = 1f, bool matchContentScaleWithCreator = false, bool deactivateContentOnHide = true, bool recurseRect = false, bool trackCreator = false, bool ignoreEventsWhileDragging = false, bool ignorePointerEvents = false, bool blockRayCasts = false, int? fontSizeOverride = null, bool useProxyTooltipCreator = false, bool ignoreClear = false, bool useOppositeDirection = false, bool clearOnDisable = false, Func<string> dynamicText = null, int? forcedMainAxisPivot = null)
	{
		if (text.IsNullOrEmpty() && dynamicText == null)
		{
			return tooltipCreator;
		}
		TooltipText orAddComponent = _GetTooltipCreator(tooltipCreator, useProxyTooltipCreator).gameObject.GetOrAddComponent<TooltipText>();
		if (fontSizeOverride.HasValue)
		{
			text = _Builder.Size(fontSizeOverride.Value).Append(text).EndSize()
				.ToString();
		}
		orAddComponent.richText = text;
		orAddComponent.backgroundEnabled = backgroundEnabled;
		orAddComponent.alignment = alignment;
		orAddComponent.dynamicText = dynamicText;
		_CreateCommon(orAddComponent, beginShowTimer, waitTime, padding, canvasEdgePadding, direction, orthogonalDirection, contentScale, matchContentScaleWithCreator, deactivateContentOnHide, recurseRect, trackCreator, ignoreEventsWhileDragging, ignorePointerEvents, blockRayCasts, ignoreClear, useOppositeDirection, useProxyTooltipCreator, clearOnDisable, forcedMainAxisPivot);
		return orAddComponent.transform;
	}

	public static void CreateDynamicTooltip(Transform tooltipCreator, Func<GameObject> getContent, bool beginShowTimer = true, float waitTime = 0.2f, float padding = 12f, float canvasEdgePadding = 12f, TooltipDirection direction = TooltipDirection.Horizontal, TooltipOrthogonalDirection orthogonalDirection = TooltipOrthogonalDirection.Center, float contentScale = 1f, bool matchContentScaleWithCreator = false, bool deactivateContentOnHide = true, bool recurseRect = false, bool trackCreator = false, bool ignoreEventsWhileDragging = false, bool ignorePointerEvents = false, bool blockRayCasts = false, bool useProxyTooltipCreator = false, bool ignoreClear = false, bool useOppositeDirection = false, bool clearOnDisable = false, int? forcedMainAxisPivot = null)
	{
		TooltipDynamicObject orAddComponent = _GetTooltipCreator(tooltipCreator, useProxyTooltipCreator).gameObject.GetOrAddComponent<TooltipDynamicObject>();
		orAddComponent.getContent = getContent;
		_CreateCommon(orAddComponent, beginShowTimer, waitTime, padding, canvasEdgePadding, direction, orthogonalDirection, contentScale, matchContentScaleWithCreator, deactivateContentOnHide, recurseRect, trackCreator, ignoreEventsWhileDragging, ignorePointerEvents, blockRayCasts, ignoreClear, useOppositeDirection, useProxyTooltipCreator, clearOnDisable, forcedMainAxisPivot);
	}

	public static void ShowTooltip(Transform tooltipCreator, float? waitTimeOverride = null)
	{
		TooltipVisibility component = tooltipCreator.GetComponent<TooltipVisibility>();
		if ((bool)component)
		{
			if (waitTimeOverride < 0f)
			{
				component.SetVisibilityImmediate(visible: true);
			}
			else
			{
				component.StartTimer(waitTimeOverride);
			}
		}
	}

	public static void HideTooltip(Transform tooltipCreator)
	{
		tooltipCreator.GetComponent<TooltipVisibility>()?.EndTimer();
	}

	public static void ClearTooltips(Transform tooltipCreator)
	{
		TooltipGenerator[] componentsInChildren = tooltipCreator.GetComponentsInChildren<TooltipGenerator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ClearTooltip();
		}
		TooltipVisibility[] componentsInChildren2 = tooltipCreator.GetComponentsInChildren<TooltipVisibility>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].EndTimer();
		}
	}

	public static void HideTooltip<T>(Transform tooltipCreator) where T : TooltipGenerator
	{
		T component = tooltipCreator.GetComponent<T>();
		if (component != null && component.enabled)
		{
			component.visibility.EndTimer();
		}
	}

	public static GameObject GetFitterBlueprint(int layer)
	{
		return TooltipBlueprint;
	}

	public void Create(GameObject content)
	{
		Hide();
		if ((bool)content)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			_tooltip = Pools.Unpool(GetFitterBlueprint(base.gameObject.layer), rectTransform).GetComponent<TooltipFitter>();
			_tooltip.tooltipCreator = rectTransform;
			_tooltip.ConnectToWorldLinker();
			_tooltip.padding = padding;
			_tooltip.canvasEdgePadding = canvasEdgePadding;
			_tooltip.direction = direction;
			_tooltip.orthogonalDirection = orthogonalDirection;
			_tooltip.contentScale = contentScale;
			_tooltip.matchContentScaleWithCreator = matchContentScaleWithCreator;
			_tooltip.deactivateContentOnDisable = deactivateContentOnHide;
			_tooltip.recurseTooltipCreatorRect = recurseRect;
			_tooltip.trackCreator = trackCreator;
			_tooltip.useOppositeDirection = useOppositeDirection;
			_tooltip.GetComponent<CanvasGroup>().blocksRaycasts = blocksRayCasts;
			_tooltip.forcedMainAxisPivot = forcedMainAxisPivot;
			content.transform.SetParent(_tooltip.transform, worldPositionStays: false);
		}
	}

	public void Hide()
	{
		if ((bool)_tooltip)
		{
			_tooltip.Hide();
			_tooltip = null;
		}
	}

	public void AddGenerator(TooltipGenerator generator)
	{
		(_generators ?? (_generators = new HashSet<TooltipGenerator>())).Add(generator);
	}

	public void RemoveGenerator(TooltipGenerator generator)
	{
		_generators?.Remove(generator);
	}
}
