using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataRefSearchView : MonoBehaviour
{
	private static System.Random _random;

	private static Dictionary<Type, Func<ContentRef, Transform, GameObject>> _TypeViews;

	private static Dictionary<Type, Action<ContentRef>> _PlayActions;

	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/DataRefSearchView";

	public StringEvent onNameChange;

	public StringEvent onCreatorChange;

	public ColorEvent onCreatorColorChange;

	public BoolEvent onHasPlayChange;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	public static Dictionary<Type, Func<ContentRef, Transform, GameObject>> TypeViews => _TypeViews ?? (_TypeViews = new Dictionary<Type, Func<ContentRef, Transform, GameObject>>());

	private static Dictionary<Type, Action<ContentRef>> PlayActions => _PlayActions ?? (_PlayActions = new Dictionary<Type, Action<ContentRef>>());

	public ContentRef dataRef { get; private set; }

	public static GameObject Create(ContentRef dataRef, Transform parent = null)
	{
		if (!TypeViews.ContainsKey(dataRef.GetType()))
		{
			return Pools.Unpool(_Blueprint, parent).GetComponent<DataRefSearchView>()._SetData(dataRef)
				.gameObject;
		}
		return TypeViews[dataRef.GetType()](dataRef, parent);
	}

	private DataRefSearchView _SetData(ContentRef dataRef)
	{
		this.dataRef = dataRef;
		onNameChange.Invoke(dataRef.name);
		ContentCreatorType creatorType = dataRef.creatorType;
		onCreatorChange.Invoke(EnumUtil.FriendlyName(creatorType));
		onCreatorColorChange.Invoke(creatorType.GetTint());
		onHasPlayChange.Invoke(PlayActions.ContainsKey(dataRef.GetType()));
		return this;
	}

	private void Awake()
	{
		TooltipCreator.CreateTextTooltip(base.transform, "Description", beginShowTimer: false, 0.2f, backgroundEnabled: true, TextAlignmentOptions.Center, 6f, 12f, TooltipDirection.Horizontal, TooltipOrthogonalDirection.Center, 1f, matchContentScaleWithCreator: false, deactivateContentOnHide: true, recurseRect: false, trackCreator: false, ignoreEventsWhileDragging: false, ignorePointerEvents: false, blockRayCasts: false, null, useProxyTooltipCreator: false, ignoreClear: false, useOppositeDirection: false, clearOnDisable: false, delegate
		{
			string text = dataRef.GetFriendlyName().NewLineIfNotEmpty();
			string text2 = (dataRef.GetDescription() ?? "").NewLineIfNotEmpty().NewLineIfNotEmpty();
			uint fileId = dataRef.key.fileId;
			return text + text2 + fileId;
		});
	}

	private void OnDisable()
	{
		dataRef = null;
	}

	public void Play()
	{
		PlayActions[dataRef.GetType()](dataRef);
	}
}
