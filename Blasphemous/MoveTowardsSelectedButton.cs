using System.Collections.Generic;
using DG.Tweening;
using Gameplay.UI.Others.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;

public class MoveTowardsSelectedButton : SerializedMonoBehaviour
{
	public float travelSeconds = 0.2f;

	[DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.CollapsedFoldout, IsReadOnly = false)]
	public Dictionary<MenuButton, RectTransform> buttonsAndPositionMarkers;

	private RectTransform rt;

	private void Start()
	{
		rt = GetComponent<RectTransform>();
		foreach (MenuButton key in buttonsAndPositionMarkers.Keys)
		{
			key.OnMenuButtonSelected += Item_OnMenuButtonSelected;
		}
	}

	private void Item_OnMenuButtonSelected(MenuButton obj)
	{
		rt.DOKill();
		RectTransform rectTransform = buttonsAndPositionMarkers[obj];
		base.transform.SetParent(rectTransform.parent, worldPositionStays: false);
		rt.DOAnchorPos(rectTransform.anchoredPosition, travelSeconds).SetEase(Ease.InOutQuad);
	}
}
