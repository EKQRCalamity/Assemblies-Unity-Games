using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : AbstractPausableComponent
{
	public enum Mode
	{
		Text,
		ListChoice
	}

	public enum DisplayState
	{
		Hidden,
		FadeIn,
		Showing,
		WaitForSelection,
		FadeOut
	}

	private const int REGULAR_ARROW_PADDING = 8;

	private const int KOREAN_ARROW_PADDING = 6;

	private const int JAP_CHI_ARROW_PADDING = 5;

	private const float DEFAULT_TIME = 2f;

	private const float FADE_TIME = 0.07f;

	private const float END_TIME = 0.25f;

	private const float ARROW_WAIT_TIME = 0.125f;

	private const float MAX_RANDOM_OFFSET = 0.05f;

	private const int MAX_CHOICES_PER_COLUMN = 4;

	private const int COLUMN_PADDING = 55;

	private const int COLUMN_SPACING = 45;

	private const int DEFAULT_PADDING = 30;

	private const int SMALL_PADDING = 20;

	private const float TAIL_POSITION_X = -73f;

	private const float CURSOR_OFFSET_H = 10f;

	public static SpeechBubble Instance;

	[SerializeField]
	private TextMeshProUGUI mainText;

	[SerializeField]
	private TextMeshProUGUI choiceText;

	[SerializeField]
	private VerticalLayoutGroup layout;

	[SerializeField]
	private Image tail;

	[SerializeField]
	private List<Sprite> tailVariants;

	[SerializeField]
	private RectTransform arrowBox;

	[SerializeField]
	private Image arrow;

	[SerializeField]
	private Image cursor;

	[SerializeField]
	private RectTransform cursorRoot;

	[SerializeField]
	private RectTransform box;

	[SerializeField]
	private List<Sprite> arrowVariants;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private List<RectTransform> bullets;

	private float maxWidth = 558f;

	private Vector2 arrowAnchoredPosition;

	public Vector2 basePosition;

	public Vector2 panPosition;

	private int currentChoiceIndex;

	private int hideOptionBitmask;

	public string setBossRefText = string.Empty;

	public int maxLines = -1;

	public bool tailOnTheLeft;

	public bool expandOnTheRight;

	private CupheadInput.AnyPlayerInput input;

	public bool waitForRealease;

	public bool waitForFade;

	public bool hideTail;

	private Coroutine showCoroutine;

	[SerializeField]
	private LayoutElement textLayoutElement;

	private bool waiting;

	private bool delayedShow;

	private DialoguerTextData data;

	[HideInInspector]
	public bool preventQuit;

	public Mode mode { get; private set; }

	public DisplayState displayState { get; private set; }

	protected override void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Instance = this;
		}
		arrowAnchoredPosition = arrowBox.anchoredPosition;
		panPosition = base.transform.position;
		base.Awake();
		Dialoguer.Initialize();
	}

	private void Start()
	{
		if (expandOnTheRight)
		{
			base.rectTransform.anchorMin = Vector2.zero;
			base.rectTransform.anchorMax = Vector2.zero;
			base.rectTransform.pivot = Vector2.zero;
		}
		else
		{
			base.rectTransform.anchorMin = new Vector2(1f, 0f);
			base.rectTransform.anchorMax = new Vector2(1f, 0f);
			base.rectTransform.pivot = Vector2.one;
		}
		canvasGroup.alpha = 0f;
		basePosition = base.rectTransform.position;
		input = new CupheadInput.AnyPlayerInput();
		AddDialoguerEvents();
	}

	private int ProcessChoice(int playerSelection)
	{
		int num = 0;
		int num2 = 0;
		while (num2 <= playerSelection)
		{
			if (!OptionHidden(num))
			{
				num2++;
			}
			num++;
		}
		return num - 1;
	}

	private void Update()
	{
		if ((!(MapEventNotification.Current == null) && MapEventNotification.Current.showing) || waiting || waitForFade)
		{
			return;
		}
		if (input.GetButtonUp(CupheadButton.Accept))
		{
			waitForRealease = false;
		}
		if (!waitForRealease && input.GetButtonDown(CupheadButton.Accept))
		{
			if (currentChoiceIndex >= 0)
			{
				if (displayState == DisplayState.WaitForSelection)
				{
					AudioManager.Play("level_menu_select");
				}
				Dialoguer.ContinueDialogue(ProcessChoice(currentChoiceIndex));
			}
			else
			{
				Dialoguer.ContinueDialogue();
			}
		}
		if (displayState == DisplayState.WaitForSelection && input.GetButtonDown(CupheadButton.Cancel))
		{
			Dialoguer.EndDialogue();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveDialoguerEvents();
		Instance = null;
	}

	private void OnLanguageChanged()
	{
		delayedShow = true;
	}

	private void OnEnable()
	{
		if (delayedShow)
		{
			delayedShow = false;
			Show(data.text);
		}
	}

	private string ProcessTextPreShow(string text)
	{
		string normalizedText = GetNormalizedText(Localization.Translate(text).SanitizedText());
		TMP_FontAsset fontAsset = Localization.Instance.fonts[(int)Localization.language][27].fontAsset;
		TMP_FontAsset fontAsset2 = Localization.Instance.fonts[0][27].fontAsset;
		return (!(fontAsset == fontAsset2)) ? normalizedText : AdjustSpacingInFont(StringVariantGenerator.Instance.Generate(normalizedText));
	}

	public void Show(string text)
	{
		if (showCoroutine != null)
		{
			StopCoroutine(showCoroutine);
		}
		string text2 = ProcessTextPreShow(text);
		int num = 8;
		if (Localization.language == Localization.Languages.Japanese || Localization.language == Localization.Languages.SimplifiedChinese)
		{
			num = 5;
		}
		else if (Localization.language == Localization.Languages.Korean)
		{
			num = 6;
		}
		showCoroutine = StartCoroutine(show_cr(Mode.Text, text2 + "<space=" + num + "em> ", null));
	}

	public void Show(string text, List<string> listItems)
	{
		if (showCoroutine != null)
		{
			StopCoroutine(showCoroutine);
		}
		string text2 = ProcessTextPreShow(text);
		showCoroutine = StartCoroutine(show_cr(Mode.ListChoice, text2, listItems));
	}

	public void Dismiss()
	{
		StartCoroutine(dismiss_cr(preventQuit));
	}

	protected virtual string GetNormalizedText(string text)
	{
		string text2 = mainText.text;
		TMP_FontAsset font = mainText.font;
		text = text.Replace("{DEATHS}", "<size=15> </size><font=\"CupheadVogue-BoldSDF\"><b><size=36>" + PlayerData.Data.DeathCount(PlayerId.Any).ToStringInvariant() + "</size></b></font><size=15> </size>");
		text = text.Replace("{BOSSREF}", setBossRefText);
		string text3 = text;
		if (Localization.language != Localization.Languages.Japanese)
		{
			text3 = string.Empty;
			text = text.Replace("\n", " ");
			text = text.Replace(" ", "<space=11.19853> ");
			text = text.Replace("{BR}", "\n");
			mainText.text = text;
			mainText.font = Localization.Instance.fonts[(int)Localization.language][27].fontAsset;
			mainText.CalculateLayoutInputHorizontal();
			string text4 = string.Empty;
			int num = 10000;
			int num2 = 0;
			while (mainText.text.Length > 0 && num > 0)
			{
				num--;
				while (mainText.text.Length > 0 && mainText.preferredWidth > maxWidth && num > 0)
				{
					num--;
					string text5 = mainText.text.Substring(mainText.text.Length - 1, 1);
					if (text5.Equals(" "))
					{
						int num3 = mainText.text.LastIndexOf("<");
						text4 = mainText.text.Substring(num3, mainText.text.Length - num3) + text4;
						mainText.text = mainText.text.Substring(0, num3);
					}
					else
					{
						text4 = text5 + text4;
						mainText.text = mainText.text.Substring(0, mainText.text.Length - 1);
					}
					mainText.CalculateLayoutInputHorizontal();
				}
				int num4 = mainText.text.LastIndexOf(" ");
				if (num4 == -1 || string.IsNullOrEmpty(text4))
				{
					text3 = ((string.IsNullOrEmpty(text4) || !text4.Substring(0, 1).Equals("<")) ? (text3 + mainText.text) : (text3 + mainText.text + "\n"));
				}
				else
				{
					text4 = mainText.text.Substring(num4 + 1) + text4;
					text3 = text3 + mainText.text.Substring(0, num4) + "\n";
				}
				mainText.text = text4;
				mainText.CalculateLayoutInputHorizontal();
				text4 = string.Empty;
				num2++;
			}
			if (num == 0)
			{
				Debug.LogError("THE WHILES ARE DEAD, BAD CODE !!!");
			}
			if (maxLines != -1 && num2 > maxLines)
			{
				text3 = text3.Replace("\n", " ");
				mainText.enableAutoSizing = true;
				textLayoutElement.enabled = true;
				layout.padding.left = 20;
				layout.padding.right = 20;
				layout.padding.bottom = 20;
				layout.padding.top = 20;
			}
			else
			{
				mainText.enableAutoSizing = false;
				textLayoutElement.enabled = false;
			}
		}
		else
		{
			mainText.enableAutoSizing = false;
			textLayoutElement.enabled = false;
		}
		mainText.text = text2;
		mainText.font = font;
		return text3;
	}

	private string AdjustSpacingInFont(string text)
	{
		string empty = string.Empty;
		empty = text.Replace("<space=11.19853>\n", "\n");
		empty = empty.Replace("<space=11.19853>]", "<space=0.01244>]");
		return empty.Replace("<space=11.19853>}", "<space=-0.00622>}");
	}

	private IEnumerator show_cr(Mode mode, string text, List<string> listItems)
	{
		waitForFade = true;
		if (displayState != 0)
		{
			yield return StartCoroutine(dismiss_cr(watchPreventQuit: false));
		}
		if (expandOnTheRight)
		{
			box.GetComponent<RectTransform>().pivot = Vector2.zero;
		}
		else
		{
			box.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
		}
		layout.padding.left = 30;
		layout.padding.right = 30;
		layout.padding.bottom = 30;
		layout.padding.top = 30;
		layout.spacing = 0f;
		mainText.text = text;
		mainText.font = Localization.Instance.fonts[(int)Localization.language][27].fontAsset;
		choiceText.font = mainText.font;
		foreach (RectTransform bullet in bullets)
		{
			bullet.gameObject.SetActive(value: false);
		}
		currentChoiceIndex = -1;
		if (mode == Mode.ListChoice)
		{
			string choiceColumn = string.Empty;
			for (int i = 0; i < listItems.Count; i++)
			{
				choiceColumn = ((i >= listItems.Count - 1) ? (choiceColumn + Localization.Translate(listItems[i]).SanitizedText()) : (choiceColumn + Localization.Translate(listItems[i]).SanitizedText() + "\n"));
			}
			if (Localization.language != Localization.Languages.Korean)
			{
				choiceText.text = StringVariantGenerator.Instance.Generate(choiceColumn);
			}
			else
			{
				choiceText.text = choiceColumn;
			}
			currentChoiceIndex = 0;
			layout.spacing = 30f;
			yield return null;
		}
		else
		{
			choiceText.text = null;
		}
		if (tailOnTheLeft)
		{
			tail.rectTransform.anchorMin = Vector2.zero;
			tail.rectTransform.anchorMax = Vector2.zero;
			tail.rectTransform.anchoredPosition = new Vector2(73f, tail.rectTransform.anchoredPosition.y);
		}
		else
		{
			tail.rectTransform.anchorMin = new Vector2(1f, 0f);
			tail.rectTransform.anchorMax = new Vector2(1f, 0f);
			tail.rectTransform.anchoredPosition = new Vector2(-73f, tail.rectTransform.anchoredPosition.y);
		}
		arrow.color = new Color(1f, 1f, 1f, 0f);
		float maxOffset = 0.05f;
		if (CupheadLevelCamera.Current != null)
		{
			maxOffset *= 100f;
		}
		base.rectTransform.position = basePosition + new Vector2(Random.Range(0f - maxOffset, maxOffset), Random.Range(0f - maxOffset, maxOffset)) * base.rectTransform.localScale.x;
		tail.sprite = tailVariants.RandomChoice();
		tail.enabled = !hideTail;
		arrow.sprite = arrowVariants.RandomChoice();
		base.animator.Play("Idle", 0, Random.Range(0f, 1f));
		base.animator.Play("Idle", 1, Random.Range(0f, 1f));
		displayState = DisplayState.FadeIn;
		yield return StartCoroutine(fade_cr(canvasGroup.alpha, 1f));
		yield return CupheadTime.WaitForSeconds(this, 0.125f);
		displayState = DisplayState.Showing;
		showCoroutine = null;
		Color colorHidden = new Color(1f, 1f, 1f, 0f);
		Color colorShown = new Color(1f, 1f, 1f, 1f);
		if (expandOnTheRight)
		{
			arrowBox.anchoredPosition = new Vector2(arrowAnchoredPosition.x + box.sizeDelta.x, arrowBox.anchoredPosition.y);
		}
		if (mode == Mode.Text)
		{
			arrow.color = ((!waiting) ? colorShown : colorHidden);
			cursor.color = colorHidden;
		}
		else
		{
			cursor.color = ((!waiting) ? colorShown : colorHidden);
			displayState = DisplayState.WaitForSelection;
			waitForFade = false;
			while (displayState == DisplayState.WaitForSelection)
			{
				if (waiting)
				{
					yield return null;
					continue;
				}
				if (PauseManager.state != PauseManager.State.Paused)
				{
					if (input.GetButtonDown(CupheadButton.MenuDown) && currentChoiceIndex < listItems.Count - 1)
					{
						currentChoiceIndex++;
						base.animator.SetTrigger("MoveDown");
						AudioManager.Play("level_menu_move");
					}
					if (input.GetButtonDown(CupheadButton.MenuUp) && currentChoiceIndex > 0)
					{
						currentChoiceIndex--;
						base.animator.SetTrigger("MoveUp");
						AudioManager.Play("level_menu_move");
					}
				}
				cursorRoot.anchoredPosition = getCursorPos(currentChoiceIndex, listItems.Count);
				cursor.color = colorShown;
				yield return null;
			}
		}
		waitForFade = false;
		cursor.color = colorHidden;
	}

	private IEnumerator dismiss_cr(bool watchPreventQuit)
	{
		if (displayState == DisplayState.Hidden)
		{
			yield break;
		}
		while (displayState == DisplayState.FadeIn)
		{
			yield return null;
		}
		if (watchPreventQuit)
		{
			while (preventQuit)
			{
				yield return null;
			}
		}
		displayState = DisplayState.FadeOut;
		yield return StartCoroutine(fade_cr(canvasGroup.alpha, 0f));
		displayState = DisplayState.Hidden;
	}

	private IEnumerator fade_cr(float startOpacity, float endOpacity)
	{
		if (endOpacity == 0f)
		{
			canvasGroup.alpha = endOpacity;
			yield break;
		}
		yield return null;
		float t = 0f;
		while (t < 0.07f)
		{
			yield return null;
			t += (float)CupheadTime.Delta;
			canvasGroup.alpha = Mathf.Lerp(startOpacity, endOpacity, t / 0.07f);
		}
		canvasGroup.alpha = endOpacity;
	}

	private Vector2 getCursorPos(int choiceIndex, int choiceCount)
	{
		float num = choiceText.bounds.extents.y / (float)choiceCount * 2f;
		return new Vector2(choiceText.margin.x - 10f, 0f) + Vector2.up * (((float)choiceCount - 1f) / 2f) * num + Vector2.down * ((float)choiceIndex * num);
	}

	private void setOpacity(float opacity)
	{
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onStarted += OnDialogueStartedHandler;
		Dialoguer.events.onEnded += OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded += OnDialogueInstantlyEndedHandler;
		Dialoguer.events.onTextPhase += OnDialogueTextPhaseHandler;
		Dialoguer.events.onWindowClose += OnDialogueWindowCloseHandler;
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onStarted -= OnDialogueStartedHandler;
		Dialoguer.events.onEnded -= OnDialogueEndedHandler;
		Dialoguer.events.onInstantlyEnded -= OnDialogueInstantlyEndedHandler;
		Dialoguer.events.onTextPhase -= OnDialogueTextPhaseHandler;
		Dialoguer.events.onWindowClose -= OnDialogueWindowCloseHandler;
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	private void OnDialogueStartedHandler()
	{
		Localization.OnLanguageChangedEvent += OnLanguageChanged;
		if (Map.Current != null)
		{
			Map.Current.CurrentState = Map.State.Event;
		}
		if (CupheadMapCamera.Current != null)
		{
			CupheadMapCamera.Current.MoveToPosition(panPosition, 0.75f, 1f);
		}
		if (MapUIVignetteDialogue.Current != null)
		{
			MapUIVignetteDialogue.Current.FadeIn();
		}
	}

	private void OnDialogueEndedHandler()
	{
		Localization.OnLanguageChangedEvent -= OnLanguageChanged;
		Dismiss();
	}

	private void OnDialogueInstantlyEndedHandler()
	{
		Localization.OnLanguageChangedEvent -= OnLanguageChanged;
		Dismiss();
	}

	private void OnDialogueTextPhaseHandler(DialoguerTextData data)
	{
		this.data = data;
		if (data.choices == null)
		{
			Show(data.text);
		}
		else
		{
			if (data.choices.Length <= 0)
			{
				return;
			}
			List<string> list = new List<string>();
			for (int i = 0; i < data.choices.Length; i++)
			{
				if (!OptionHidden(i))
				{
					list.Add(data.choices[i]);
				}
			}
			Show(data.text, list);
		}
	}

	public void ClearHideOptionBitmask()
	{
		hideOptionBitmask = 0;
	}

	public void HideOptionByIndex(int i)
	{
		hideOptionBitmask |= 1 << i;
	}

	private bool OptionHidden(int i)
	{
		return (hideOptionBitmask & (1 << i)) != 0;
	}

	private void OnDialogueWindowCloseHandler()
	{
		Dismiss();
		ClearHideOptionBitmask();
		if (MapUIVignetteDialogue.Current != null)
		{
			MapUIVignetteDialogue.Current.FadeOut();
		}
		if (Map.Current != null && Map.Current.CurrentState != Map.State.Graveyard)
		{
			Map.Current.CurrentState = Map.State.Ready;
		}
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "Wait")
		{
			StartCoroutine(wait_cr(Parser.FloatParse(metadata)));
		}
	}

	private IEnumerator wait_cr(float waitDuration)
	{
		waiting = true;
		arrow.color = new Color(1f, 1f, 1f, 0f);
		while (waitDuration > 0f)
		{
			yield return null;
			waitDuration -= (float)CupheadTime.Delta;
		}
		waiting = false;
		arrow.color = new Color(1f, 1f, 1f, 1f);
	}
}
