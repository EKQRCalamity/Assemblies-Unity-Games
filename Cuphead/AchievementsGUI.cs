using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AchievementsGUI : AbstractMonoBehaviour
{
	[Serializable]
	public class IconRow
	{
		public AchievementIcon[] achievementIcons;
	}

	private static readonly string[] AchievementNames = Enum.GetNames(typeof(LocalAchievementsManager.Achievement));

	private static readonly Color UnlockedTextColor = new Color(0.9098039f, 0.8235294f, 58f / 85f);

	private static readonly Color LockedTextColor = new Color(23f / 85f, 4f / 15f, 0.2627451f);

	private static readonly string TitleHidden = "? ? ? ? ? ? ?";

	private static readonly FontLoader.FontType TitleHiddenFont = FontLoader.FontType.CupheadMemphis_Medium_merged;

	private static readonly string DescriptionHidden = "?  ?  ?  ?  ?  ?";

	private static readonly FontLoader.FontType DescriptionHiddenFont = FontLoader.FontType.CupheadVogue_Bold_merged;

	private static readonly Vector2Int GridSize = new Vector2Int(7, 6);

	private static readonly Vector2Int VisualGridSize = new Vector2Int(7, 4);

	[SerializeField]
	private IconRow[] iconRows;

	[SerializeField]
	private RectTransform cursor;

	[SerializeField]
	private Image topArrow;

	[SerializeField]
	private Image bottomArrow;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image unearnedBackground;

	[SerializeField]
	private Text titleText;

	[SerializeField]
	private Text descriptionText;

	[SerializeField]
	private LocalizationHelper titleLocalization;

	[SerializeField]
	private LocalizationHelper descriptionLocalization;

	[SerializeField]
	private Image largeIcon;

	[SerializeField]
	private Image noise;

	[SerializeField]
	private Sprite[] arrowSprites;

	private CupheadInput.AnyPlayerInput input;

	private float timeSinceStart;

	private Vector2Int achievementIndex;

	private Vector2Int cursorIndex;

	private int rowOffset;

	private SpriteAtlas defaultAtlas;

	private SpriteAtlas dlcAtlas;

	private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

	private CupheadButton activeNavigationButton = CupheadButton.None;

	private StringBuilder stringBuilder;

	private Coroutine arrowCoroutine;

	private bool dlcEnabled;

	private Vector2Int currentGridSize;

	private CanvasGroup canvasGroup;

	public bool achievementsMenuOpen { get; private set; }

	public bool inputEnabled { get; private set; }

	public bool justClosed { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		defaultAtlas = AssetLoader<SpriteAtlas>.GetCachedAsset("Achievements");
		stringBuilder = new StringBuilder();
		background.sprite = defaultAtlas.GetSprite("cheev_bg");
		unearnedBackground.sprite = defaultAtlas.GetSprite("cheev_card_unearned");
		achievementsMenuOpen = false;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
	}

	public void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	private void Update()
	{
		justClosed = false;
		timeSinceStart += Time.deltaTime;
		if (timeSinceStart < 0.25f)
		{
			return;
		}
		if (activeNavigationButton != CupheadButton.None && input.GetButtonUp(activeNavigationButton))
		{
			activeNavigationButton = CupheadButton.None;
		}
		if (!inputEnabled)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.Cancel))
		{
			AudioManager.Play("level_menu_select");
			HideAchievements();
		}
		if (activeNavigationButton != CupheadButton.None)
		{
			return;
		}
		if (GetButtonDown(CupheadButton.MenuUp))
		{
			if (achievementIndex.y == 0)
			{
				rowOffset = currentGridSize.y - VisualGridSize.y;
				cursorIndex.y = VisualGridSize.y - 1;
				achievementIndex.y = currentGridSize.y - 1;
			}
			else
			{
				if (cursorIndex.y == 0)
				{
					rowOffset--;
				}
				cursorIndex.y = Mathf.Max(cursorIndex.y - 1, 0);
				achievementIndex.y = Mathf.Max(achievementIndex.y - 1, 0);
			}
			refreshIcons();
			updateSelection();
		}
		else if (GetButtonDown(CupheadButton.MenuDown))
		{
			if (achievementIndex.y == currentGridSize.y - 1)
			{
				rowOffset = 0;
				cursorIndex.y = 0;
				achievementIndex.y = 0;
			}
			else
			{
				if (cursorIndex.y == VisualGridSize.y - 1)
				{
					rowOffset = Mathf.Min(rowOffset + 1, currentGridSize.y - VisualGridSize.y);
				}
				cursorIndex.y = Mathf.Min(cursorIndex.y + 1, VisualGridSize.y - 1);
				achievementIndex.y = Mathf.Min(achievementIndex.y + 1, currentGridSize.y - 1);
			}
			refreshIcons();
			updateSelection();
		}
		else if (GetButtonDown(CupheadButton.MenuLeft))
		{
			cursorIndex.x--;
			if (cursorIndex.x < 0)
			{
				cursorIndex.x = VisualGridSize.x - 1;
			}
			achievementIndex.x--;
			if (achievementIndex.x < 0)
			{
				achievementIndex.x = currentGridSize.x - 1;
			}
			updateSelection();
		}
		else if (GetButtonDown(CupheadButton.MenuRight))
		{
			cursorIndex.x++;
			if (cursorIndex.x >= VisualGridSize.x)
			{
				cursorIndex.x = 0;
			}
			achievementIndex.x++;
			if (achievementIndex.x >= currentGridSize.x)
			{
				achievementIndex.x = 0;
			}
			updateSelection();
		}
	}

	public void ShowAchievements()
	{
		handleDLCStatus();
		cursorIndex = Vector2Int.zero;
		achievementIndex = Vector2Int.zero;
		rowOffset = 0;
		refreshIcons();
		updateSelection();
		if (dlcEnabled)
		{
			arrowCoroutine = StartCoroutine(arrow_cr());
		}
		timeSinceStart = 0f;
		achievementsMenuOpen = true;
		canvasGroup.alpha = 1f;
		FrameDelayedCallback(interactable, 1);
	}

	public void HideAchievements()
	{
		if (dlcEnabled)
		{
			StopCoroutine(arrowCoroutine);
			arrowCoroutine = null;
		}
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
		achievementsMenuOpen = false;
		justClosed = true;
	}

	private void interactable()
	{
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		inputEnabled = true;
	}

	private void updateSelection()
	{
		AchievementIcon achievementIcon = iconRows[cursorIndex.y].achievementIcons[cursorIndex.x];
		cursor.position = achievementIcon.transform.position;
		int num = achievementIndex.y * currentGridSize.x + achievementIndex.x;
		LocalAchievementsManager.Achievement achievement = (LocalAchievementsManager.Achievement)num;
		string text = achievement.ToString();
		bool flag = LocalAchievementsManager.IsAchievementUnlocked(achievement);
		bool flag2 = LocalAchievementsManager.IsHiddenAchievement(achievement);
		if (flag || !flag2)
		{
			string key = "Achievement" + text + "Title";
			titleLocalization.ApplyTranslation(Localization.Find(key));
			string key2 = "Achievement" + text + "Desc";
			descriptionLocalization.ApplyTranslation(Localization.Find(key2));
		}
		else
		{
			titleText.text = TitleHidden;
			titleText.font = FontLoader.GetFont(TitleHiddenFont);
			descriptionText.text = DescriptionHidden;
			descriptionText.font = FontLoader.GetFont(DescriptionHiddenFont);
		}
		string text2 = text;
		if (flag)
		{
			text2 += "_earned";
		}
		Sprite achievementSprite = getAchievementSprite(text2, achievement);
		largeIcon.sprite = achievementSprite;
		titleText.color = ((!flag) ? LockedTextColor : UnlockedTextColor);
		descriptionText.color = ((!flag) ? LockedTextColor : UnlockedTextColor);
		unearnedBackground.enabled = !flag;
		noise.sprite = getSprite((!flag) ? "cheev_card_noise_unearned" : "cheev_card_noise_earned", defaultAtlas);
		AudioManager.Play("level_menu_move");
	}

	protected bool GetButtonDown(CupheadButton button)
	{
		if (input.GetButtonDown(button))
		{
			activeNavigationButton = button;
			return true;
		}
		return false;
	}

	private void handleDLCStatus()
	{
		dlcEnabled = DLCManager.DLCEnabled();
		currentGridSize = ((!dlcEnabled) ? VisualGridSize : GridSize);
		if (dlcEnabled && dlcAtlas == null)
		{
			dlcAtlas = AssetLoader<SpriteAtlas>.GetCachedAsset("Achievements_DLC");
		}
	}

	private IEnumerator arrow_cr()
	{
		int index = 0;
		WaitForFrameTimePersistent wait = new WaitForFrameTimePersistent(1f / 12f, useUnalteredTime: true);
		while (true)
		{
			topArrow.sprite = arrowSprites[index];
			index = MathUtilities.NextIndex(index, arrowSprites.Length);
			bottomArrow.sprite = arrowSprites[MathUtilities.NextIndex(index, arrowSprites.Length)];
			yield return wait;
		}
	}

	private void refreshIcons()
	{
		if (dlcEnabled)
		{
			topArrow.enabled = rowOffset != 0;
			bottomArrow.enabled = rowOffset != 2;
		}
		else
		{
			Image image = topArrow;
			bool flag = false;
			bottomArrow.enabled = flag;
			image.enabled = flag;
		}
		int num = rowOffset * currentGridSize.x;
		IconRow[] array = iconRows;
		foreach (IconRow iconRow in array)
		{
			AchievementIcon[] achievementIcons = iconRow.achievementIcons;
			foreach (AchievementIcon achievementIcon in achievementIcons)
			{
				stringBuilder.Length = 0;
				LocalAchievementsManager.Achievement achievement = (LocalAchievementsManager.Achievement)num;
				stringBuilder.Append(AchievementNames[num]);
				if (LocalAchievementsManager.IsAchievementUnlocked(achievement))
				{
					stringBuilder.Append("_earned");
				}
				stringBuilder.Append("_sm");
				Sprite achievementSprite = getAchievementSprite(stringBuilder.ToString(), achievement);
				achievementIcon.SetIcon(achievementSprite);
				num++;
			}
		}
	}

	private Sprite getSprite(string spriteName, SpriteAtlas atlas)
	{
		if (!spriteCache.TryGetValue(spriteName, out var value))
		{
			value = atlas.GetSprite(spriteName);
			spriteCache.Add(spriteName, value);
		}
		return value;
	}

	private Sprite getAchievementSprite(string spriteName, LocalAchievementsManager.Achievement achievement)
	{
		SpriteAtlas atlas = ((Array.IndexOf(LocalAchievementsManager.DLCAchievements, achievement) < 0) ? defaultAtlas : dlcAtlas);
		return getSprite(spriteName, atlas);
	}
}
