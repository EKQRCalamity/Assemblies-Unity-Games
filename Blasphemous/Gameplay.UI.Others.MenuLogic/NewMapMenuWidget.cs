using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using FMODUnity;
using Framework.Managers;
using Framework.Map;
using Rewired;
using Sirenix.OdinInspector;
using Tools.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewMapMenuWidget : MonoBehaviour
{
	[BoxGroup("Elements", true, false, 0)]
	public Text DistrictText;

	[BoxGroup("Elements", true, false, 0)]
	public BasicUIBlockingWidget ParentWidget;

	[BoxGroup("Elements", true, false, 0)]
	public Text ZoneText;

	[BoxGroup("Elements", true, false, 0)]
	public Text PercentText;

	[BoxGroup("Elements", true, false, 0)]
	public Text CherubsText;

	[BoxGroup("Elements", true, false, 0)]
	public Transform MapContent;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject CursorElement;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject CursorTeleportElement;

	[BoxGroup("Config", true, false, 0)]
	public List<MapRendererConfig> RendererConfigs;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject MapControlsRootNormal;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject MapControlsRootTeleport;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject MapControlsNotMap;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject selectMarkRoot;

	[BoxGroup("Elements", true, false, 0)]
	public Image MarkFrameImage;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject markTemplateElement;

	[BoxGroup("Elements", true, false, 0)]
	public GameObject noMapText;

	[BoxGroup("Elements", true, false, 0)]
	public InputIcon zoomIconInput;

	[BoxGroup("Mark Buttons", true, false, 0)]
	public GameObject addMarkNormalButton;

	[BoxGroup("Mark Buttons", true, false, 0)]
	public GameObject addMarkDisableButton;

	[BoxGroup("Mark Buttons", true, false, 0)]
	public GameObject removeMarkButton;

	[BoxGroup("Teleport Buttons", true, false, 0)]
	public GameObject teleportNormalButton;

	[BoxGroup("Teleport Buttons", true, false, 0)]
	public GameObject teleportDisableButton;

	[BoxGroup("Config Teleport", true, false, 0)]
	public bool UseSpeedInTeleport = true;

	[BoxGroup("Config Teleport", true, false, 0)]
	[ShowIf("UseSpeedInTeleport", true)]
	public float TeleportSpeed = 400f;

	[HideIf("UseSpeedInTeleport", true)]
	[BoxGroup("Config Teleport", true, false, 0)]
	public float TeleportTime = 0.4f;

	[BoxGroup("Config Teleport", true, false, 0)]
	public bool AllowChangeInTween;

	[BoxGroup("Config Teleport", true, false, 0)]
	public Ease TeleportEase = Ease.InOutQuad;

	[BoxGroup("Config Teleport", true, false, 0)]
	public float TeleportMovementEpsilon = 0.2f;

	[BoxGroup("Config Teleport", true, false, 0)]
	[EventRef]
	public string TeleportSoundFound = "event:/SFX/UI/ChangeTab";

	[BoxGroup("Config Teleport", true, false, 0)]
	[EventRef]
	public string TeleportSoundNotFound = "event:/SFX/UI/ChangeSelection";

	[BoxGroup("Config Teleport", true, false, 0)]
	public List<float> AnglesToCheckNear = new List<float> { 30f, 40f, 60f };

	private List<CellData> Cells = new List<CellData>();

	private Dictionary<CellKey, CellData> CellsDict = new Dictionary<CellKey, CellData>();

	private Dictionary<ZoneKey, List<CellKey>> KeysByZone = new Dictionary<ZoneKey, List<CellKey>>();

	private List<MapRenderer> MapRenderers = new List<MapRenderer>();

	private int CurrentRendererIndex;

	private MapRenderer CurrentRenderer;

	private Player Rewired;

	private ScrollRect scrollRect;

	private bool canAddMark;

	private bool canRemoveMark;

	private CellKey CurrentKey;

	private CellData CurrentCell;

	private bool IsEnableMarkSelector;

	private List<NewMapMenuWidgetMarkItem> markItems = new List<NewMapMenuWidgetMarkItem>();

	private int currentMarkItem;

	private const float MovementEpsilon = 0.08f;

	private int visibleMarksInScroll;

	private float markUISize;

	private bool mapEnabled;

	private bool teleportMoving;

	private CellKey TeleportTarget = new CellKey(0, 0);

	private Tweener TeleportTween;

	private Tweener TeleportCursorTween;

	private RectTransform TeleportTransform;

	private Vector2 TeleportStart = Vector2.zero;

	private bool canPlayTeleportNotFoundSound;

	private float markCurrentCooldownTime;

	private const float MARK_MAX_COOLDOWN_TIME = 0.5f;

	public PauseWidget.MapModes CurrentMapMode { get; private set; }

	public void Initialize()
	{
		TeleportTransform = (RectTransform)CursorTeleportElement.transform;
		TeleportStart = TeleportTransform.sizeDelta;
		Player player = ReInput.players.GetPlayer(0);
		if (player != null)
		{
			Rewired = player;
		}
		Cells = Core.NewMapManager.GetAllRevealedCells();
		List<CellKey> allRevealSecretsCells = Core.NewMapManager.GetAllRevealSecretsCells();
		CellsDict.Clear();
		KeysByZone.Clear();
		Dictionary<CellKey, List<MapData.MarkType>> allMarks = Core.NewMapManager.GetAllMarks();
		foreach (CellData cell in Cells)
		{
			CellsDict[cell.CellKey] = cell;
			if (!KeysByZone.ContainsKey(cell.ZoneId))
			{
				KeysByZone[cell.ZoneId] = new List<CellKey>();
			}
			KeysByZone[cell.ZoneId].Add(cell.CellKey);
		}
		int num = 0;
		if (Core.NewMapManager.GetTotalCells() != 0)
		{
			List<CellData> list = Cells.Where((CellData cell) => !cell.IgnoredForMapPercentage).ToList();
			num = list.Count * 100 / Core.NewMapManager.GetTotalCells();
		}
		PercentText.text = num + "%";
		CherubsText.text = CherubCaptorPersistentObject.CountRescuedCherubs() + "/" + 38;
		if (MapRenderers.Count == 0)
		{
			int num2 = 0;
			foreach (MapRendererConfig rendererConfig in RendererConfigs)
			{
				MapRenderer mapRenderer = new MapRenderer(rendererConfig, MapContent, num2.ToString());
				mapRenderer.Render(Cells, allRevealSecretsCells, allMarks, Core.NewMapManager.GetPlayerCell());
				mapRenderer.Root.gameObject.SetActive(value: false);
				MapRenderers.Add(mapRenderer);
				num2++;
			}
		}
		else
		{
			foreach (MapRenderer mapRenderer2 in MapRenderers)
			{
				mapRenderer2.Render(Cells, allRevealSecretsCells, allMarks, Core.NewMapManager.GetPlayerCell());
				mapRenderer2.Root.gameObject.SetActive(value: false);
			}
		}
		CurrentRendererIndex = 0;
		CurrentRenderer = null;
		markItems.Clear();
		currentMarkItem = -1;
		markTemplateElement.SetActive(value: false);
		scrollRect = selectMarkRoot.GetComponentInChildren<ScrollRect>();
		markUISize = ((RectTransform)markTemplateElement.transform).rect.width + 2f;
		float width = scrollRect.viewport.rect.width;
		visibleMarksInScroll = Mathf.FloorToInt(width / markUISize);
	}

	public void OnShow(PauseWidget.MapModes mapMode)
	{
		Core.NewMapManager.RevealCellInCurrentPlayerPosition();
		CurrentMapMode = mapMode;
		CurrentRendererIndex = 0;
		UpdateZoomControl();
		IsEnableMarkSelector = false;
		mapEnabled = Core.NewMapManager.CanShowMapInCurrentZone();
		UpdateCurrentRenderer();
		MapControlsRootNormal.SetActive(mapEnabled && CurrentMapMode == PauseWidget.MapModes.SHOW);
		MapControlsRootTeleport.SetActive(mapEnabled && CurrentMapMode == PauseWidget.MapModes.TELEPORT);
		MapControlsNotMap.SetActive(!mapEnabled);
		teleportNormalButton.SetActive(value: false);
		teleportDisableButton.SetActive(value: true);
		markCurrentCooldownTime = 0f;
	}

	public void ZoomOut()
	{
		if (mapEnabled && CurrentRendererIndex < MapRenderers.Count - 1)
		{
			CurrentRendererIndex++;
			UpdateCurrentRenderer();
			CurrentRenderer.UpdateMarks(Core.NewMapManager.GetAllMarks());
			UpdateZoomControl();
		}
	}

	public void ZoomIn()
	{
		if (mapEnabled && CurrentRendererIndex > 0)
		{
			CurrentRendererIndex--;
			UpdateCurrentRenderer();
			CurrentRenderer.UpdateMarks(Core.NewMapManager.GetAllMarks());
			UpdateZoomControl();
		}
	}

	public void CenterView()
	{
		CurrentRenderer.ResetSelection();
		CurrentCell = null;
		CurrentKey = Core.NewMapManager.GetPlayerCell();
		if (CurrentRenderer.Config.CenterAtPlayer)
		{
			CurrentRenderer.SetCenterCell(CurrentKey);
		}
		else
		{
			CurrentRenderer.SetCenterCell(new CellKey(CurrentRenderer.Config.centerCell.x, CurrentRenderer.Config.centerCell.y));
		}
		UpdateCellData();
	}

	public void UITabLeft()
	{
		if (IsEnableMarkSelector && currentMarkItem != -1 && currentMarkItem > 0)
		{
			markItems[currentMarkItem].SetSelected(selected: false);
			currentMarkItem--;
			markItems[currentMarkItem].SetSelected(selected: true);
			UpdateScrollToCurrent();
		}
	}

	public void UITabRight()
	{
		if (IsEnableMarkSelector && currentMarkItem != -1 && currentMarkItem < markItems.Count - 1)
		{
			markItems[currentMarkItem].SetSelected(selected: false);
			currentMarkItem++;
			markItems[currentMarkItem].SetSelected(selected: true);
			UpdateScrollToCurrent();
		}
	}

	public void ToogleMarks()
	{
		if (mapEnabled && CurrentRenderer != null && CurrentRenderer.Config.UseMarks)
		{
			IsEnableMarkSelector = !IsEnableMarkSelector;
			UpdateElementsByMode();
		}
	}

	public void SubmitPressed()
	{
		switch (CurrentMapMode)
		{
		case PauseWidget.MapModes.SHOW:
			MarkPressed();
			break;
		case PauseWidget.MapModes.TELEPORT:
			TeleportPressed();
			break;
		}
	}

	public void MarkPressed()
	{
		if (mapEnabled && CurrentRenderer != null && CurrentRenderer.Config.UseMarks && IsEnableMarkSelector && CurrentMapMode != PauseWidget.MapModes.TELEPORT && ParentWidget.IsActive() && !ParentWidget.IsFading && !(markCurrentCooldownTime < 0.5f))
		{
			if (canAddMark)
			{
				Core.NewMapManager.AddMarkOnCell(CurrentRenderer.GetCenterCell(), markItems[currentMarkItem].MarkId);
			}
			else if (canRemoveMark)
			{
				Core.NewMapManager.RemoveMarkOnCell(CurrentRenderer.GetCenterCell());
			}
			CurrentRenderer.UpdateMarks(Core.NewMapManager.GetAllMarks());
		}
	}

	public void TeleportPressed()
	{
		if (mapEnabled && CurrentRenderer != null && CurrentMapMode == PauseWidget.MapModes.TELEPORT)
		{
			CellData teleportCell = GetTeleportCell();
			if (teleportCell != null)
			{
				UIController.instance.HidePauseMenu();
				Core.SpawnManager.SpawnFromTeleportOnPrieDieu(teleportCell.ZoneId.GetLevelName(), useFade: true);
			}
		}
	}

	public bool GoBack()
	{
		return false;
	}

	private void UpdateCurrentRenderer()
	{
		if (CurrentRenderer != null)
		{
			CurrentRenderer.ResetSelection();
			CurrentRenderer.Root.gameObject.SetActive(value: false);
		}
		if (mapEnabled)
		{
			CurrentRenderer = MapRenderers[CurrentRendererIndex];
			CurrentRenderer.Root.gameObject.SetActive(value: true);
			CursorElement.SetActive(CurrentRenderer.Config.UseMarks && CurrentMapMode != PauseWidget.MapModes.TELEPORT);
			CursorTeleportElement.SetActive(CurrentRenderer.Config.UseMarks && CurrentMapMode == PauseWidget.MapModes.TELEPORT);
			CenterView();
		}
		else
		{
			CurrentRenderer = null;
			CurrentKey = null;
			CursorElement.SetActive(value: false);
			CursorTeleportElement.SetActive(value: false);
		}
		noMapText.SetActive(!mapEnabled);
		UpdateCellData();
		IsEnableMarkSelector = CurrentRenderer != null && CurrentRenderer.Config.UseMarks && CurrentMapMode == PauseWidget.MapModes.SHOW;
		currentMarkItem = -1;
		UpdateElementsByMode();
	}

	private void UpdateElementsByMode()
	{
		if (!mapEnabled)
		{
			IsEnableMarkSelector = false;
			if (CurrentRenderer != null)
			{
				CurrentRenderer.SetVisibleMarks(visible: false);
			}
		}
		else
		{
			CurrentRenderer.SetVisibleMarks(CurrentRenderer.Config.UseMarks && (CurrentMapMode == PauseWidget.MapModes.TELEPORT || IsEnableMarkSelector));
			if (CurrentRenderer.Config.UseMarks)
			{
				foreach (Transform item in markTemplateElement.transform.parent)
				{
					if (item != markTemplateElement.transform)
					{
						Object.Destroy(item.gameObject);
					}
				}
				markItems.Clear();
				foreach (KeyValuePair<MapData.MarkType, Sprite> mark in CurrentRenderer.Config.Marks)
				{
					if (!MapData.MarkPrivate.Contains(mark.Key))
					{
						GameObject gameObject = Object.Instantiate(markTemplateElement, markTemplateElement.transform.parent);
						gameObject.name = mark.Key.ToString();
						NewMapMenuWidgetMarkItem component = gameObject.GetComponent<NewMapMenuWidgetMarkItem>();
						component.SetInitialData(mark.Key, mark.Value, selected: false);
						gameObject.SetActive(value: true);
						markItems.Add(component);
					}
				}
				if (markItems.Count > 0)
				{
					if (currentMarkItem < 0 || currentMarkItem >= markItems.Count)
					{
						markItems[0].SetSelected(selected: true);
						currentMarkItem = 0;
					}
					else
					{
						markItems[currentMarkItem].SetSelected(selected: true);
					}
					UpdateScrollToCurrent();
				}
			}
		}
		UpdateMarkButtons();
	}

	private void UpdateMarkButtons()
	{
		addMarkNormalButton.SetActive(IsEnableMarkSelector && canAddMark);
		removeMarkButton.SetActive(IsEnableMarkSelector && canRemoveMark);
		addMarkDisableButton.SetActive(!IsEnableMarkSelector || (!canAddMark && !canRemoveMark));
		foreach (NewMapMenuWidgetMarkItem markItem in markItems)
		{
			markItem.SetDisabled(!IsEnableMarkSelector);
		}
		MarkFrameImage.color = (IsEnableMarkSelector ? Color.white : Color.gray);
	}

	private void Update()
	{
		if (CurrentRenderer == null || !CurrentRenderer.Config || !ParentWidget.IsActive())
		{
			return;
		}
		if (Rewired != null && CurrentMapMode == PauseWidget.MapModes.SHOW)
		{
			float axisRaw = Rewired.GetAxisRaw(48);
			float axisRaw2 = Rewired.GetAxisRaw(49);
			if (Mathf.Abs(axisRaw) >= 0.08f || Mathf.Abs(axisRaw2) >= 0.08f)
			{
				Vector2 vector = new Vector2(axisRaw, axisRaw2);
				vector *= Time.unscaledDeltaTime * CurrentRenderer.Config.MovementSpeed * -1f;
				CurrentRenderer.MoveCenter(vector);
			}
		}
		if (CurrentRenderer.Config.CenterAtPlayer)
		{
			CellKey centerCell = CurrentRenderer.GetCenterCell();
			if (centerCell != CurrentKey)
			{
				CurrentKey = centerCell;
				UpdateCellData();
			}
		}
		if (CurrentRenderer.Config.UseMarks && IsEnableMarkSelector)
		{
			canAddMark = false;
			CellKey centerCell2 = CurrentRenderer.GetCenterCell();
			canRemoveMark = Core.NewMapManager.IsMarkOnCell(centerCell2);
			if (!canRemoveMark)
			{
				canAddMark = Core.NewMapManager.CanAddMark(centerCell2);
			}
			if (markCurrentCooldownTime < 0.5f)
			{
				markCurrentCooldownTime += Time.unscaledDeltaTime;
			}
			UpdateMarkButtons();
		}
		if (CurrentMapMode == PauseWidget.MapModes.SHOW)
		{
			ControlZoom();
			return;
		}
		ControlTeleportMovement();
		bool flag = GetTeleportCell() != null;
		teleportNormalButton.SetActive(flag);
		teleportDisableButton.SetActive(!flag);
	}

	private CellData GetTeleportCell()
	{
		CellData result = null;
		CellKey centerCell = CurrentRenderer.GetCenterCell();
		if (!centerCell.Equals(Core.NewMapManager.GetPlayerCell()) && CellsDict.ContainsKey(centerCell) && CellsDict[centerCell].Type == EditorMapCellData.CellType.PrieDieu)
		{
			result = CellsDict[centerCell];
		}
		return result;
	}

	private void ControlZoom()
	{
		if (Rewired != null)
		{
			float axisRaw = Rewired.GetAxisRaw(20);
			int num = CurrentRendererIndex;
			float num2 = 0.2f;
			if (axisRaw > num2 && CurrentRendererIndex != 0)
			{
				num = 0;
			}
			else if (axisRaw < 0f - num2 && CurrentRendererIndex == 0)
			{
				num = 1;
			}
			if (num != CurrentRendererIndex)
			{
				CurrentRendererIndex = num;
				UpdateCurrentRenderer();
				CurrentRenderer.UpdateMarks(Core.NewMapManager.GetAllMarks());
				UpdateZoomControl();
			}
		}
	}

	private void ControlTeleportMovement()
	{
		if (teleportMoving && !AllowChangeInTween)
		{
			return;
		}
		AnglesToCheckNear.Sort();
		List<CellKey> list = new List<CellKey>();
		List<float> list2 = new List<float>();
		for (int i = 0; i < AnglesToCheckNear.Count; i++)
		{
			list.Add(new CellKey(0, 0));
			list2.Add(0f);
		}
		float axisRaw = Rewired.GetAxisRaw(48);
		float axisRaw2 = Rewired.GetAxisRaw(49);
		Vector2 to = new Vector2(axisRaw, axisRaw2);
		if (to.magnitude >= TeleportMovementEpsilon)
		{
			to.Normalize();
			CellKey centerCell = CurrentRenderer.GetCenterCell();
			Vector2Int vector = centerCell.GetVector2();
			foreach (KeyValuePair<CellKey, CellData> item in CellsDict)
			{
				if (centerCell.Equals(item.Key) || item.Value.Type != EditorMapCellData.CellType.PrieDieu)
				{
					continue;
				}
				Vector2Int vector2 = item.Key.GetVector2();
				Vector2Int vector2Int = vector2 - vector;
				float num = Vector2.Angle(vector2Int, to);
				float num2 = vector2Int.sqrMagnitude;
				for (int j = 0; j < AnglesToCheckNear.Count; j++)
				{
					if (num <= AnglesToCheckNear[j] && (list2[j] == 0f || num2 < list2[j]))
					{
						list2[j] = num2;
						list[j] = item.Key;
					}
				}
			}
			string text = TeleportSoundNotFound;
			for (int k = 0; k < AnglesToCheckNear.Count; k++)
			{
				if (list2[k] != 0f)
				{
					text = TeleportSoundFound;
					MoveTeleport(list[k]);
					break;
				}
			}
			if (!(text != string.Empty))
			{
				return;
			}
			if (text == TeleportSoundNotFound)
			{
				if (!canPlayTeleportNotFoundSound)
				{
					return;
				}
				canPlayTeleportNotFoundSound = false;
			}
			Core.Audio.PlayOneShot(text);
		}
		else
		{
			canPlayTeleportNotFoundSound = true;
		}
	}

	private void MoveTeleport(CellKey target)
	{
		if (teleportMoving)
		{
			if (!AllowChangeInTween || TeleportTarget.Equals(target))
			{
				return;
			}
			if (TeleportTween != null)
			{
				TeleportTween.Kill();
			}
		}
		EndCursor();
		teleportMoving = true;
		TeleportTarget = target;
		Vector2 vector = -CurrentRenderer.GetPosition(target);
		float num = TeleportTime;
		if (UseSpeedInTeleport)
		{
			Vector2 vector2 = CurrentRenderer.Center;
			num = (vector - vector2).magnitude / TeleportSpeed;
		}
		DOTween.defaultTimeScaleIndependent = true;
		TeleportTween = DOTween.To(() => CurrentRenderer.Center, delegate(Vector3Wrapper x)
		{
			CurrentRenderer.Center = x;
		}, new Vector3(vector.x, vector.y, 0f), num).OnComplete(EndMoveTeleport).SetEase(TeleportEase);
		TeleportCursorTween = DOTween.To(() => TeleportTransform.sizeDelta, delegate(Vector2Wrapper x)
		{
			TeleportTransform.sizeDelta = x;
		}, TeleportStart * 2f, num / 2f).SetLoops(2, LoopType.Yoyo);
	}

	private void EndMoveTeleport()
	{
		teleportMoving = false;
	}

	private void EndCursor()
	{
		if (TeleportCursorTween != null && TeleportCursorTween.IsActive())
		{
			TeleportCursorTween.Kill();
		}
		TeleportTransform.sizeDelta = TeleportStart;
	}

	private void UpdateZoomControl()
	{
		zoomIconInput.axisCheck = ((CurrentRendererIndex != 0) ? InputIcon.AxisCheck.Positive : InputIcon.AxisCheck.Negative);
		zoomIconInput.RefreshIcon();
	}

	private void UpdateCellData()
	{
		CellData currentCell = CurrentCell;
		if (CurrentKey == null || !CellsDict.TryGetValue(CurrentKey, out CurrentCell))
		{
			CurrentCell = null;
		}
		if (currentCell != null && (CurrentCell == null || currentCell.ZoneId != CurrentCell.ZoneId))
		{
			SetSelection(currentCell.ZoneId, value: false);
		}
		if (CurrentCell != null && (currentCell == null || currentCell.ZoneId != CurrentCell.ZoneId))
		{
			SetSelection(CurrentCell.ZoneId, value: true);
		}
		if (CurrentCell == null)
		{
			DistrictText.text = string.Empty;
			ZoneText.text = string.Empty;
		}
		else
		{
			DistrictText.text = Core.NewMapManager.GetDistrictName(CurrentCell.ZoneId.District).ToUpper();
			ZoneText.text = Core.NewMapManager.GetZoneName(CurrentCell.ZoneId);
		}
	}

	private void UpdateScrollToCurrent()
	{
		Canvas.ForceUpdateCanvases();
		int num = Mathf.FloorToInt(currentMarkItem / visibleMarksInScroll);
		float num2 = (float)(num * visibleMarksInScroll) * markUISize;
		scrollRect.content.anchoredPosition = new Vector2(0f - num2, scrollRect.content.anchoredPosition.y);
	}

	private void SetSelection(ZoneKey zone, bool value)
	{
		if (KeysByZone.ContainsKey(zone) && CurrentRenderer != null)
		{
			CurrentRenderer.SetSelected(KeysByZone[zone], value);
		}
	}
}
