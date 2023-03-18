using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerHealthPE02 : MonoBehaviour
{
	public static float FillAmount;

	[SerializeField]
	private Color fillingColor;

	[SerializeField]
	private List<GameObject> stocks;

	[SerializeField]
	private List<GameObject> links;

	private int lastMaxStocks = -1;

	private float prevFrameFillAmount = -1f;

	private Penitent penitent;

	private static int CurrentStocks
	{
		get
		{
			float f = Core.Logic.Penitent.Stats.Life.Current / 30f;
			return Mathf.CeilToInt(f);
		}
	}

	private static int MaxStocks => Mathf.CeilToInt(Core.Logic.Penitent.Stats.Life.CurrentMax / 30f);

	private int HealthAsStocks
	{
		get
		{
			float current = penitent.Stats.Life.Current;
			return Mathf.CeilToInt(current / 30f);
		}
	}

	public static float StocksDamage
	{
		get
		{
			float num = Core.Logic.Penitent.Stats.Life.Current % 30f;
			return (!(Mathf.Abs(num) < Mathf.Epsilon)) ? num : 30f;
		}
	}

	public static float StocksHeal
	{
		get
		{
			int num = CurrentStocks + 1;
			float num2 = Mathf.Min((float)num * 30f, Core.Logic.Penitent.Stats.Life.CurrentMax);
			return num2 - Core.Logic.Penitent.Stats.Life.Current;
		}
	}

	private void Awake()
	{
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		base.enabled = false;
	}

	private void OnDestroy()
	{
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		base.enabled = false;
		penitent = null;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		if (Core.ready && Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.MENU))
		{
			lastMaxStocks = -1;
			prevFrameFillAmount = -1f;
			penitent = null;
		}
		else
		{
			base.enabled = true;
			penitent = Core.Logic.Penitent;
			FillAmount = 0f;
		}
	}

	private void Update()
	{
		if (!(penitent == null))
		{
			UpdateDisplayedStocks();
			UpdateFilledStocks();
			UpdateLinks();
			UpdateLastStockFillAmount();
			prevFrameFillAmount = FillAmount;
		}
	}

	public void ForceUpdate()
	{
		lastMaxStocks = -1;
		prevFrameFillAmount = -1f;
		penitent = Core.Logic.Penitent;
	}

	private void UpdateDisplayedStocks()
	{
		int maxStocks = MaxStocks;
		if (maxStocks == lastMaxStocks)
		{
			return;
		}
		lastMaxStocks = maxStocks;
		stocks.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
		links.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
		for (int i = 0; i < maxStocks; i++)
		{
			stocks[i].SetActive(value: true);
			if (i > 0)
			{
				links[i - 1].SetActive(value: true);
			}
		}
	}

	private void UpdateFilledStocks()
	{
		int currentlyFilledStocks = GetCurrentlyFilledStocks();
		if (currentlyFilledStocks != HealthAsStocks)
		{
			for (int i = 0; i < stocks.Count; i++)
			{
				GameObject gameObject = stocks[i];
				Transform child = gameObject.transform.GetChild(0);
				Image component = child.GetComponent<Image>();
				component.fillAmount = 1f;
				component.color = Color.white;
				child.gameObject.SetActive(i < HealthAsStocks);
			}
		}
	}

	private void UpdateLinks()
	{
		for (int i = 1; i < links.Count; i++)
		{
			float alpha = ((i - 1 >= HealthAsStocks) ? 0f : 1f);
			links[i - 1].GetComponent<Image>().canvasRenderer.SetAlpha(alpha);
		}
	}

	private void UpdateLastStockFillAmount()
	{
		if (prevFrameFillAmount != FillAmount)
		{
			prevFrameFillAmount = FillAmount;
			if (FillAmount >= 1f)
			{
				Image component = stocks[HealthAsStocks - 1].transform.GetChild(0).GetComponent<Image>();
				component.color = Color.white;
				component.fillAmount = 1f;
				FillAmount = 0f;
			}
			else if (FillAmount > 0f && FillAmount < 1f)
			{
				stocks[HealthAsStocks].transform.GetChild(0).gameObject.SetActive(value: true);
				Image component2 = stocks[HealthAsStocks].transform.GetChild(0).GetComponent<Image>();
				component2.color = fillingColor;
				component2.fillAmount = FillAmount;
			}
			else
			{
				stocks[HealthAsStocks].transform.GetChild(0).gameObject.SetActive(value: false);
			}
		}
	}

	private int GetCurrentlyFilledStocks()
	{
		return stocks.FindAll((GameObject x) => x.transform.GetChild(0).gameObject.activeSelf && x.transform.GetChild(0).GetComponent<Image>().fillAmount == 1f).Count;
	}
}
