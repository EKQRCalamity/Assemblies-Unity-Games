using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Penitences;
using Gameplay.UI.Others.MenuLogic;
using Gameplay.UI.Others.UIGameLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class GameplayWidget : UIWidget
{
	[SerializeField]
	private GameObject leftPartRoot;

	[SerializeField]
	private GameObject rightPartRoot;

	[SerializeField]
	private GameObject[] keys = new GameObject[4];

	[SerializeField]
	private PlayerPurgePoints purgePoints;

	[SerializeField]
	private PlayerPurgePoints purgePointsDemake;

	[SerializeField]
	private PlayerFervour fervour;

	[SerializeField]
	private BossRushTimer bossRushTimer;

	[SerializeField]
	private MiriamTimer miriamTimer;

	[SerializeField]
	private List<GameObject> normalHealthGameObjects;

	[SerializeField]
	private List<GameObject> pe02HealthGameObjects;

	[SerializeField]
	private List<GameObject> demakeHealthGameObjects;

	[SerializeField]
	private List<GameObject> deactivateInDemakeGameObjects;

	[SerializeField]
	private PlayerHealth normalPlayerHealth;

	[SerializeField]
	private PlayerHealthPE02 pe02PlayerHealth;

	[SerializeField]
	private PlayerHealthDemake demakePlayerHealth;

	[SerializeField]
	private Image CurrentPenitence;

	[SerializeField]
	[BoxGroup("Penitence", true, false, 0)]
	private List<SelectSaveSlots.PenitenceData> PenitencesConfig;

	private CanvasGroup canvas;

	private bool demakeUiIsActive;

	private void Awake()
	{
		canvas = GetComponent<CanvasGroup>();
		PenitenceManager.OnPenitenceChanged += OnPenitenceChanged;
		OnPenitenceChanged(Core.PenitenceManager.GetCurrentPenitence(), null);
	}

	private void OnDestroy()
	{
		PenitenceManager.OnPenitenceChanged -= OnPenitenceChanged;
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoadDemake;
	}

	public void RestoreDefaultPanelsStatus()
	{
		miriamTimer.Hide();
		bossRushTimer.Hide();
		purgePointsDemake.gameObject.SetActive(value: true);
		ShowPurgePoints();
	}

	private void OnPenitenceChanged(IPenitence current, List<IPenitence> completed)
	{
		bool isPE02Active = current is PenitencePE02;
		normalHealthGameObjects.ForEach(delegate(GameObject x)
		{
			x.SetActive(!isPE02Active);
		});
		pe02HealthGameObjects.ForEach(delegate(GameObject x)
		{
			x.SetActive(isPE02Active);
		});
		normalPlayerHealth.enabled = !isPE02Active;
		pe02PlayerHealth.enabled = isPE02Active;
		if (isPE02Active)
		{
			pe02PlayerHealth.ForceUpdate();
		}
		Sprite sprite = null;
		if (current != null)
		{
			foreach (SelectSaveSlots.PenitenceData item in PenitencesConfig)
			{
				if (item.id.ToUpper() == current.Id.ToUpper())
				{
					sprite = item.InProgress;
				}
			}
		}
		CurrentPenitence.enabled = sprite != null;
		CurrentPenitence.sprite = sprite;
	}

	private void OnBeforeLevelLoadDemake(Level oldLevel, Level newLevel)
	{
		if (newLevel.LevelName.StartsWith("D25"))
		{
			demakeUiIsActive = false;
		}
		else
		{
			demakeUiIsActive = true;
		}
	}

	private void Update()
	{
		LogicManager logic = Core.Logic;
		UIManager uI = Core.UI;
		if (logic == null || uI == null)
		{
			return;
		}
		bool flag = !logic.IsMenuScene() && !logic.IsAttrackScene() && uI.MustShowGamePlayUI();
		canvas.alpha = ((!flag) ? 0f : 1f);
		if (flag)
		{
			for (int i = 0; i < 4; i++)
			{
				keys[i].SetActive(Core.InventoryManager.CheckBossKey(i));
			}
		}
		bool enableDemakeUi = Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.DEMAKE);
		UpdateDemakeUI(enableDemakeUi);
	}

	private void UpdateDemakeUI(bool enableDemakeUi)
	{
		if (demakeUiIsActive && !enableDemakeUi)
		{
			demakeUiIsActive = false;
			purgePointsDemake.gameObject.SetActive(value: false);
			purgePoints.gameObject.SetActive(value: true);
			deactivateInDemakeGameObjects.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: true);
			});
			IPenitence currentPenitence = Core.PenitenceManager.GetCurrentPenitence();
			if (currentPenitence != null && currentPenitence is PenitencePE02)
			{
				normalHealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: false);
				});
				pe02HealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: true);
				});
				demakeHealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: false);
				});
				normalPlayerHealth.enabled = false;
				pe02PlayerHealth.enabled = true;
				demakePlayerHealth.enabled = false;
			}
			else
			{
				normalHealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: true);
				});
				pe02HealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: false);
				});
				demakeHealthGameObjects.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: false);
				});
				normalPlayerHealth.enabled = true;
				pe02PlayerHealth.enabled = false;
				demakePlayerHealth.enabled = false;
			}
		}
		else if (!demakeUiIsActive && enableDemakeUi)
		{
			demakeUiIsActive = true;
			deactivateInDemakeGameObjects.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
			normalHealthGameObjects.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
			pe02HealthGameObjects.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
			demakeHealthGameObjects.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: true);
			});
			normalPlayerHealth.enabled = false;
			pe02PlayerHealth.enabled = false;
			demakePlayerHealth.enabled = true;
			purgePointsDemake.gameObject.SetActive(value: true);
			purgePoints.gameObject.SetActive(value: false);
			LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoadDemake;
			LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoadDemake;
		}
	}

	public void UpdatePurgePoints()
	{
		if (purgePoints.gameObject.activeInHierarchy)
		{
			purgePoints.RefreshPoints(inmediate: true);
		}
		else if (purgePointsDemake.gameObject.activeInHierarchy)
		{
			purgePointsDemake.RefreshPoints(inmediate: true);
		}
	}

	public void ShowPurgePoints()
	{
		purgePoints.gameObject.SetActive(value: true);
	}

	public void ShowLeftPart()
	{
		leftPartRoot.SetActive(value: true);
	}

	public void HideLeftPart()
	{
		leftPartRoot.SetActive(value: false);
	}

	public void ShowRightPart()
	{
		rightPartRoot.SetActive(value: true);
	}

	public void HideRightPart()
	{
		rightPartRoot.SetActive(value: false);
	}

	public void HidePurgePoints()
	{
		purgePoints.gameObject.SetActive(value: false);
	}

	public void ShowBossRushTimer()
	{
		bossRushTimer.Show();
	}

	public void HideBossRushTimer()
	{
		bossRushTimer.Hide();
	}

	public void UpdateGuiltLevel(bool whenDead)
	{
		if (purgePoints.gameObject.activeInHierarchy)
		{
			purgePoints.RefreshGuilt(whenDead);
		}
		if (fervour.gameObject.activeInHierarchy)
		{
			fervour.RefreshGuilt(whenDead);
		}
	}

	public void NotEnoughFervour()
	{
		fervour.NotEnoughFervour();
	}

	public void StartMiriamTimer()
	{
		miriamTimer.StartTimer();
	}

	public void StopMiriamTimer()
	{
		miriamTimer.StopTimer(completed: true);
	}

	public void SetMiriamTimerTargetTime(float targetTime)
	{
		miriamTimer.SetTargetTime(targetTime);
	}

	public void ShowMiriamTimer()
	{
		miriamTimer.Show();
	}

	public void HideMiriamTimer()
	{
		miriamTimer.Hide();
	}
}
