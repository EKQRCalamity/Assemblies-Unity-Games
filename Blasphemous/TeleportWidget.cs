using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Others.MenuLogic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TeleportWidget : BasicUIBlockingWidget
{
	public delegate void TeleportCancelDelegate();

	private class TeleportObject
	{
		public Image image;

		public TeleportDestination destination;

		public EventsButton button;
	}

	[BoxGroup("Graphics", true, false, 0)]
	public Sprite teleportSprite;

	[BoxGroup("Graphics", true, false, 0)]
	public Sprite selectedTeleportSprite;

	[BoxGroup("UI", true, false, 0)]
	public Transform teleportRoot;

	[BoxGroup("UI", true, false, 0)]
	public Text teleportDestinationText;

	[BoxGroup("UI", true, false, 0)]
	public bool displaceTravelButon = true;

	[BoxGroup("UI", true, false, 0)]
	public RectTransform travelButton;

	[BoxGroup("UI", true, false, 0)]
	public Vector2 travelButtonOffset = new Vector2(4f, 0f);

	[BoxGroup("UI Effects", true, false, 0)]
	public Ease movementEaseType = Ease.Linear;

	[BoxGroup("UI Effects", true, false, 0)]
	public float movementTime = 0.2f;

	private bool isCanceled;

	private readonly List<TeleportObject> teleports = new List<TeleportObject>();

	private int currentSelected = -1;

	private bool firstSelectionAfterShow = true;

	public static event TeleportCancelDelegate OnTeleportCancelled;

	protected override void OnWidgetShow()
	{
		base.OnWidgetShow();
		isCanceled = false;
		firstSelectionAfterShow = true;
		for (int i = 0; i < teleportRoot.childCount; i++)
		{
			teleportRoot.GetChild(i).gameObject.SetActive(value: false);
		}
		teleports.Clear();
		int num = 0;
		int slot = 0;
		foreach (TeleportDestination allUIActiveTeleport in Core.SpawnManager.GetAllUIActiveTeleports())
		{
			GameObject gameObject = teleportRoot.GetChild(allUIActiveTeleport.selectedSlot).gameObject;
			gameObject.name = $"Teleport_{num}: {allUIActiveTeleport.teleportName}";
			gameObject.SetActive(value: true);
			Selectable component = gameObject.GetComponent<Selectable>();
			TeleportObject teleportObject = new TeleportObject();
			teleportObject.image = gameObject.GetComponent<Image>();
			teleportObject.destination = allUIActiveTeleport;
			teleportObject.button = component.GetComponent<EventsButton>();
			TeleportObject teleportObject2 = teleportObject;
			int elementNumber = num;
			teleportObject2.button.onSelected = new EventsButton.ButtonSelectedEvent();
			teleportObject2.button.onSelected.AddListener(delegate
			{
				SelectElement(elementNumber);
			});
			teleportObject2.button.onClick = new EventsButton.ButtonClickedEvent();
			teleportObject2.button.onClick.AddListener(delegate
			{
				ActivateElement(elementNumber);
			});
			if (teleportObject2.destination.sceneName == Core.NewMapManager.CurrentScene.GetLevelName())
			{
				slot = num;
			}
			teleports.Add(teleportObject2);
			num++;
		}
		UpdateNavigation();
		StartCoroutine(FocusSlotSecure(slot));
	}

	private void UpdateNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		foreach (TeleportObject teleport in teleports)
		{
			list.Add(teleport.button);
		}
		foreach (TeleportObject teleport2 in teleports)
		{
			UpdateNavigationFor(teleport2.button, list);
		}
	}

	private void UpdateNavigationFor(Selectable s, List<Selectable> selectables)
	{
		Navigation navigation = s.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnUp = s.FindSelectableFromList(Vector2.up, selectables);
		navigation.selectOnDown = s.FindSelectableFromList(Vector2.down, selectables);
		navigation.selectOnLeft = s.FindSelectableFromList(Vector2.left, selectables);
		navigation.selectOnRight = s.FindSelectableFromList(Vector2.right, selectables);
		s.navigation = navigation;
	}

	private void ActivateElement(int slot)
	{
		if (currentSelected >= 0 && currentSelected < teleports.Count)
		{
			TeleportObject teleportObject = teleports[currentSelected];
			if (teleportObject.destination.sceneName.Equals(Core.LevelManager.currentLevel.LevelName))
			{
				isCanceled = true;
				FadeHide();
			}
			else
			{
				Core.SpawnManager.Teleport(teleportObject.destination);
				FadeHide();
			}
		}
	}

	private void SelectElement(int elementNumber)
	{
		foreach (TeleportObject teleport in teleports)
		{
			teleport.image.overrideSprite = teleportSprite;
			teleport.image.SetNativeSize();
		}
		int targetIdx = elementNumber;
		Transform transform = teleports[targetIdx].image.transform;
		Vector2 sizeDelta = travelButton.sizeDelta;
		Vector3 vector = ((!teleports[targetIdx].destination.labelUnderIcon) ? ((Vector3)travelButtonOffset) : new Vector3((0f - sizeDelta.x) * 0.5f, 0f - sizeDelta.y - 8f));
		Vector3 vector2 = transform.position + vector;
		if (displaceTravelButon && !firstSelectionAfterShow)
		{
			travelButton.DOKill();
			travelButton.DOMove(vector2, movementTime).SetEase(movementEaseType).OnComplete(delegate
			{
				foreach (TeleportObject teleport2 in teleports)
				{
					teleport2.image.overrideSprite = teleportSprite;
					teleport2.image.SetNativeSize();
				}
				UpdateOnDestination(targetIdx);
			});
		}
		else
		{
			travelButton.position = vector2;
			UpdateOnDestination(targetIdx);
		}
		firstSelectionAfterShow = false;
	}

	private void UpdateOnDestination(int targetIdx)
	{
		teleports[targetIdx].image.overrideSprite = selectedTeleportSprite;
		teleports[targetIdx].image.SetNativeSize();
		teleportDestinationText.text = Core.NewMapManager.GetZoneNameFromBundle(teleports[targetIdx].destination.sceneName);
		currentSelected = targetIdx;
	}

	private IEnumerator FocusSlotSecure(int slot, bool ignoreSound = true)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (teleports.Count > slot)
		{
			EventsButton button = teleports[slot].button;
			button.Select();
		}
	}

	private void Update()
	{
		if (ReInput.players.GetPlayer(0).GetButtonDown(51))
		{
			isCanceled = true;
		}
	}

	protected override void OnWidgetHide()
	{
		base.OnWidgetHide();
		if (isCanceled && TeleportWidget.OnTeleportCancelled != null)
		{
			TeleportWidget.OnTeleportCancelled();
		}
	}
}
