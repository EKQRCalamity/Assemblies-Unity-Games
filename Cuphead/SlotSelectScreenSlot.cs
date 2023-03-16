using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotSelectScreenSlot : AbstractMonoBehaviour
{
	[SerializeField]
	private RectTransform emptyChild;

	[SerializeField]
	private RectTransform mainChild;

	[SerializeField]
	private RectTransform mainDLCChild;

	[SerializeField]
	private TMP_Text worldMapText;

	[SerializeField]
	private TMP_Text worldMapTextDLC;

	[SerializeField]
	private Image boxImage;

	[SerializeField]
	private Image starImage;

	[SerializeField]
	private Image starImageDLC;

	[SerializeField]
	private Image starImageSelectedBase;

	[SerializeField]
	private Image starImageSelectedDLC;

	[SerializeField]
	private Image noiseImage;

	[SerializeField]
	private Sprite unselectedBoxSprite;

	[SerializeField]
	private Sprite unselectedBoxSpriteExpert;

	[SerializeField]
	private Sprite unselectedBoxSpriteComplete;

	[SerializeField]
	private Sprite unselectedBoxSpriteExpertDLC;

	[SerializeField]
	private Sprite unselectedBoxSpriteCompleteDLC;

	[SerializeField]
	private Sprite unselectedNoise;

	[SerializeField]
	private Sprite selectedBoxSpriteMugman;

	[SerializeField]
	private Sprite selectedBoxSprite;

	[SerializeField]
	private Sprite selectedBoxSpriteExpert;

	[SerializeField]
	private Sprite selectedBoxSpriteComplete;

	[SerializeField]
	private Sprite selectedBoxSpriteExpertDLC;

	[SerializeField]
	private Sprite selectedBoxSpriteCompleteDLC;

	[SerializeField]
	private Sprite selectedNoiseMugman;

	[SerializeField]
	private Sprite selectedNoise;

	[SerializeField]
	private GameObject cuphead;

	[SerializeField]
	private Animator cupheadSelect;

	[SerializeField]
	private Animator cupheadAnimator;

	[SerializeField]
	private GameObject mugman;

	[SerializeField]
	private Animator mugmanSelect;

	[SerializeField]
	private Animator mugmanAnimator;

	[SerializeField]
	private TMP_Text slotTitle;

	[SerializeField]
	private TMP_Text slotSeparator;

	[SerializeField]
	private TMP_Text slotPercentage;

	[SerializeField]
	private TMP_Text slotPercentageSelectedBase;

	[SerializeField]
	private TMP_Text slotPercentageSelectedDLC;

	[SerializeField]
	private Text emptyText;

	[SerializeField]
	private Color selectedTextColor;

	[SerializeField]
	private Color unselectedTextColor;

	private bool selectingMugman;

	private bool isExpert;

	private bool isExpertDLC;

	private bool isComplete;

	private bool isCompleteDLC;

	public bool IsEmpty { get; private set; }

	public bool isPlayer1Mugman { get; private set; }

	public Image noise
	{
		get
		{
			return noiseImage;
		}
		set
		{
			noiseImage = value;
		}
	}

	public void Init(int slotNumber)
	{
		PlayerData dataForSlot = PlayerData.GetDataForSlot(slotNumber);
		cuphead.SetActive(value: false);
		mugman.SetActive(value: false);
		if (!dataForSlot.GetMapData(Scenes.scene_map_world_1).sessionStarted && !dataForSlot.IsTutorialCompleted && dataForSlot.CountLevelsCompleted(Level.world1BossLevels) == 0)
		{
			emptyChild.gameObject.SetActive(value: true);
			mainChild.gameObject.SetActive(value: false);
			mainDLCChild.gameObject.SetActive(value: false);
			IsEmpty = true;
			return;
		}
		IsEmpty = false;
		emptyChild.gameObject.SetActive(value: false);
		mainChild.gameObject.SetActive(value: true);
		Localization.Translation translation = slotNumber switch
		{
			0 => (!dataForSlot.isPlayer1Mugman) ? Localization.Translate("TitleScreenSlot1") : Localization.Translate("TitleScreenMugmanSlot1"), 
			1 => (!dataForSlot.isPlayer1Mugman) ? Localization.Translate("TitleScreenSlot2") : Localization.Translate("TitleScreenMugmanSlot2"), 
			_ => (!dataForSlot.isPlayer1Mugman) ? Localization.Translate("TitleScreenSlot3") : Localization.Translate("TitleScreenMugmanSlot3"), 
		};
		slotTitle.text = translation.text;
		slotSeparator.font = translation.fonts.fontAsset;
		slotTitle.font = translation.fonts.fontAsset;
		isPlayer1Mugman = dataForSlot.isPlayer1Mugman;
		isExpert = dataForSlot.IsHardModeAvailable;
		isExpertDLC = dataForSlot.IsHardModeAvailableDLC;
		int num = Mathf.RoundToInt(dataForSlot.GetCompletionPercentage());
		isComplete = num == 200;
		int num2 = Mathf.RoundToInt(dataForSlot.GetCompletionPercentageDLC());
		isCompleteDLC = num2 == 100;
		slotPercentage.text = num + num2 + "%";
		if (DLCManager.DLCEnabled())
		{
			slotPercentageSelectedBase.text = num + "%";
			slotPercentageSelectedDLC.text = num2 + "%";
		}
		translation = dataForSlot.CurrentMap switch
		{
			Scenes.scene_map_world_2 => Localization.Translate("TitleScreenWorld2"), 
			Scenes.scene_map_world_3 => Localization.Translate("TitleScreenWorld3"), 
			Scenes.scene_map_world_4 => Localization.Translate("TitleScreenWorld4"), 
			Scenes.scene_map_world_DLC => (!DLCManager.DLCEnabled()) ? Localization.Translate("TitleScreenWorld1") : Localization.Translate("TitleScreenWorldDLC"), 
			_ => Localization.Translate("TitleScreenWorld1"), 
		};
		worldMapText.text = translation.text;
		worldMapText.font = translation.fonts.fontAsset;
		worldMapTextDLC.text = translation.text;
		worldMapTextDLC.font = translation.fonts.fontAsset;
	}

	public void SetSelected(bool selected)
	{
		if (DLCManager.DLCEnabled() && !IsEmpty)
		{
			mainChild.gameObject.SetActive(!selected);
			mainDLCChild.gameObject.SetActive(selected);
		}
		slotTitle.color = ((!selected) ? unselectedTextColor : selectedTextColor);
		slotSeparator.color = ((!selected) ? unselectedTextColor : selectedTextColor);
		slotPercentage.color = ((!selected) ? unselectedTextColor : selectedTextColor);
		worldMapText.color = ((!selected) ? unselectedTextColor : selectedTextColor);
		emptyText.color = ((!selected) ? unselectedTextColor : selectedTextColor);
		boxImage.sprite = ((!selected) ? unselectedBoxSprite : ((!isPlayer1Mugman) ? selectedBoxSprite : selectedBoxSpriteMugman));
		if (!IsEmpty && isComplete)
		{
			starImage.sprite = ((!selected) ? unselectedBoxSpriteComplete : selectedBoxSpriteComplete);
			starImage.gameObject.SetActive(value: true);
		}
		else if (!IsEmpty && isExpert)
		{
			starImage.sprite = ((!selected) ? unselectedBoxSpriteExpert : selectedBoxSpriteExpert);
			starImage.gameObject.SetActive(value: true);
		}
		else
		{
			starImage.gameObject.SetActive(value: false);
		}
		if (!IsEmpty && isCompleteDLC)
		{
			starImageDLC.sprite = ((!selected) ? unselectedBoxSpriteCompleteDLC : selectedBoxSpriteCompleteDLC);
			starImageDLC.gameObject.SetActive(value: true);
		}
		else if (!IsEmpty && isExpertDLC)
		{
			starImageDLC.sprite = ((!selected) ? unselectedBoxSpriteExpertDLC : selectedBoxSpriteExpertDLC);
			starImageDLC.gameObject.SetActive(value: true);
		}
		else
		{
			starImageDLC.gameObject.SetActive(value: false);
		}
		if (starImage.gameObject.activeInHierarchy && !starImageDLC.gameObject.activeInHierarchy)
		{
			starImage.transform.position = starImageDLC.transform.position;
		}
		noiseImage.sprite = ((!selected) ? unselectedNoise : ((!isPlayer1Mugman) ? selectedNoise : selectedNoiseMugman));
	}

	public string GetSlotTitle()
	{
		return slotTitle.text;
	}

	public TMP_FontAsset GetSlotTitleFont()
	{
		return slotTitle.font;
	}

	public string GetSlotSeparator()
	{
		return slotSeparator.text;
	}

	public TMP_FontAsset GetSlotSeparatorFont()
	{
		return slotSeparator.font;
	}

	public string GetSlotPercentage()
	{
		return slotPercentage.text;
	}

	public TMP_FontAsset GetSlotPercentageFont()
	{
		return slotPercentage.font;
	}

	public void EnterSelectMenu()
	{
		selectingMugman = isPlayer1Mugman;
		if (selectingMugman)
		{
			mugman.SetActive(value: true);
			mugmanSelect.Play("Zoom_In");
		}
		else
		{
			cuphead.SetActive(value: true);
			cupheadSelect.Play("Zoom_In");
		}
		mainDLCChild.gameObject.SetActive(value: false);
	}

	public void SwapSprite()
	{
		noiseImage.enabled = false;
		selectingMugman = !selectingMugman;
		cuphead.SetActive(!selectingMugman);
		mugman.SetActive(selectingMugman);
	}

	public void StopSelectingPlayer()
	{
		StartCoroutine(player_zoomout_cr());
	}

	private IEnumerator player_zoomout_cr()
	{
		if (selectingMugman)
		{
			mugmanSelect.Play("Zoom_Out");
			yield return mugmanSelect.WaitForAnimationToEnd(this, "Zoom_Out");
			mugman.SetActive(value: false);
		}
		else
		{
			cupheadSelect.Play("Zoom_Out");
			yield return cupheadSelect.WaitForAnimationToEnd(this, "Zoom_Out");
			cuphead.SetActive(value: false);
		}
		yield return null;
		selectingMugman = isPlayer1Mugman;
		noiseImage.enabled = true;
	}

	public void PlayAnimation(int slotNumber)
	{
		isPlayer1Mugman = selectingMugman;
		PlayerData dataForSlot = PlayerData.GetDataForSlot(slotNumber);
		Animator animator = ((!isPlayer1Mugman) ? cupheadAnimator : mugmanAnimator);
		if (dataForSlot.IsHardModeAvailable)
		{
			if (dataForSlot.NumCoinsCollected >= 40 && dataForSlot.NumSupers(PlayerId.PlayerOne) >= 3)
			{
				animator.Play("100Percent");
			}
			else
			{
				animator.Play("DefeatedDevil");
			}
		}
		else
		{
			animator.Play("Default");
		}
	}
}
