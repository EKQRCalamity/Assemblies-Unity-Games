using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Gameplay.UI.Others.MenuLogic;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogWidget : MonoBehaviour
{
	private enum DialogState
	{
		Off,
		FadeText,
		WaitPress,
		WaitResponse,
		WaitSound,
		ScrollInitial,
		ScrollScrolling
	}

	private const string ANIMATOR_BOOL = "SHOW_DIALOG";

	private const int MAX_RESPONSES = 2;

	private Animator animator;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject linesRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject longLinesRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject objectRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject purgeRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject purgeGenericRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject buyRoot;

	[BoxGroup("Controls", true, false, 0)]
	public RectTransform responseRoot;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject backgroundImage;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject backgroundWithResponseImage;

	[BoxGroup("Positions", true, false, 0)]
	public RectTransform normalResponsePosition;

	[BoxGroup("Positions", true, false, 0)]
	public RectTransform buyResponsePosition;

	[BoxGroup("Positions", true, false, 0)]
	public RectTransform linesWithResponsePosition;

	[BoxGroup("Positions", true, false, 0)]
	public RectTransform linesWithOutResponsePosition;

	[BoxGroup("Lines", true, false, 0)]
	public Text dialogLine;

	[BoxGroup("Lines", true, false, 0)]
	public Text dialoglongLine;

	[BoxGroup("Object", true, false, 0)]
	public Text objectFirstText;

	[BoxGroup("Object", true, false, 0)]
	public Text objectSecondText;

	[BoxGroup("Object", true, false, 0)]
	public Image objectImage;

	[BoxGroup("Object", true, false, 0)]
	public GameObject backgorund;

	[BoxGroup("Purge", true, false, 0)]
	public Text purgueFirstText;

	[BoxGroup("Purge", true, false, 0)]
	public Text purgueAmount;

	[BoxGroup("Purge Generic", true, false, 0)]
	public Text purgueGenericText;

	[BoxGroup("Buy", true, false, 0)]
	public Text buyFirstText;

	[BoxGroup("Buy", true, false, 0)]
	public Text buyAmount;

	[BoxGroup("Buy", true, false, 0)]
	public Text buyCaption;

	[BoxGroup("Buy", true, false, 0)]
	public Text buyDescrption;

	[BoxGroup("Buy", true, false, 0)]
	public Image buyImage;

	[BoxGroup("Buy", true, false, 0)]
	public CustomScrollView customScrollView;

	[BoxGroup("Scroll", true, false, 0)]
	public RectTransform rtContentRect;

	[BoxGroup("Scroll", true, false, 0)]
	public RectTransform rtScrollViewRect;

	[BoxGroup("Scroll", true, false, 0)]
	public Scrollbar scrollbar;

	[BoxGroup("Scroll", true, false, 0)]
	public float scrollInitialTime = 2f;

	[BoxGroup("Scroll", true, false, 0)]
	public float scrollSpeed = 10f;

	[BoxGroup("Response", true, false, 0)]
	public Text[] dialogResponse = new Text[2];

	[BoxGroup("Options", true, false, 0)]
	public float fadeSpeed = 2f;

	[BoxGroup("Options", true, false, 0)]
	public float noModalNextLine = 3f;

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Color optionNormalColor = new Color(0.972549f, 76f / 85f, 0.78039217f);

	[SerializeField]
	[BoxGroup("Options", true, false, 0)]
	private Color optionHighligterColor = new Color(0.80784315f, 72f / 85f, 0.49803922f);

	private DialogState state;

	private GameObject responsesUI;

	private GameObject[] dialogResponseSelection = new GameObject[2];

	private bool hasResponses;

	private float currentTextAlpha;

	private CanvasGroup longTextCanvas;

	private CanvasGroup textCanvas;

	private CanvasGroup objectCanvas;

	private CanvasGroup purgeCanvas;

	private CanvasGroup purgeGenericCanvas;

	private CanvasGroup buyCanvas;

	private CanvasGroup currentCanvas;

	private Tweener scrollbarTween;

	private TextGenerationSettings generationSettings;

	private float currentSoundLenght;

	private bool IsLongText;

	private float timeToWait;

	private float tweenFloat;

	private DialogManager.ModalMode currentModal;

	private const string ITEM_GIVE_QUESTION_MARK = "<color=#F8E4C6FF>?</color>";

	private void Awake()
	{
		textCanvas = linesRoot.GetComponent<CanvasGroup>();
		longTextCanvas = longLinesRoot.GetComponent<CanvasGroup>();
		objectCanvas = objectRoot.GetComponent<CanvasGroup>();
		purgeCanvas = purgeRoot.GetComponent<CanvasGroup>();
		buyCanvas = buyRoot.GetComponent<CanvasGroup>();
		purgeGenericCanvas = purgeGenericRoot.GetComponent<CanvasGroup>();
		linesRoot.SetActive(value: false);
		longLinesRoot.SetActive(value: false);
		objectRoot.SetActive(value: false);
		purgeRoot.SetActive(value: false);
		purgeGenericRoot.SetActive(value: false);
		buyRoot.SetActive(value: false);
		animator = GetComponent<Animator>();
		animator.SetBool("SHOW_DIALOG", value: false);
		responsesUI = dialogResponse[0].transform.parent.gameObject;
		responsesUI.SetActive(value: false);
		state = DialogState.Off;
		hasResponses = false;
		for (int i = 0; i < 2; i++)
		{
			dialogResponseSelection[i] = dialogResponse[i].transform.Find("Img").gameObject;
			dialogResponseSelection[i].SetActive(value: false);
		}
		RectTransform rectTransform = (RectTransform)dialogLine.transform;
		generationSettings = default(TextGenerationSettings);
		generationSettings.textAnchor = dialogLine.alignment;
		generationSettings.color = dialogLine.color;
		generationSettings.generationExtents = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
		generationSettings.pivot = Vector2.zero;
		generationSettings.richText = dialogLine.supportRichText;
		generationSettings.font = dialogLine.font;
		generationSettings.fontSize = dialogLine.fontSize;
		generationSettings.fontStyle = FontStyle.Normal;
		generationSettings.verticalOverflow = dialogLine.verticalOverflow;
		backgorund.SetActive(value: false);
	}

	private void Update()
	{
		switch (state)
		{
		case DialogState.FadeText:
		{
			bool flag = false;
			currentTextAlpha += fadeSpeed * Time.deltaTime;
			if (currentTextAlpha >= 1f)
			{
				currentTextAlpha = 1f;
				flag = true;
			}
			currentCanvas.alpha = currentTextAlpha;
			if (flag)
			{
				if (IsLongText)
				{
					timeToWait = scrollInitialTime;
					state = DialogState.ScrollInitial;
					scrollbar.value = 1f;
				}
				else
				{
					EndTransitionText();
				}
			}
			break;
		}
		case DialogState.WaitSound:
			timeToWait -= Time.deltaTime;
			if (timeToWait <= 0f)
			{
				Core.Dialog.UIEvent_LineEnded();
			}
			break;
		case DialogState.ScrollInitial:
			timeToWait -= Time.deltaTime;
			if (timeToWait <= 0f)
			{
				state = DialogState.ScrollScrolling;
				float num = rtContentRect.rect.height - rtScrollViewRect.rect.height;
				float duration = num / scrollSpeed;
				tweenFloat = 1f;
				scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
				{
					tweenFloat = x;
				}, 0f, duration).SetEase(Ease.Linear).OnComplete(EndTransitionText)
					.SetId("AutoScroll");
			}
			break;
		case DialogState.ScrollScrolling:
			scrollbar.value = tweenFloat;
			break;
		case DialogState.WaitPress:
		case DialogState.WaitResponse:
			break;
		}
	}

	private void EndTransitionText()
	{
		if (hasResponses)
		{
			state = DialogState.WaitResponse;
			responsesUI.SetActive(value: true);
			EventSystem.current.SetSelectedGameObject(dialogResponse[0].gameObject);
			StartCoroutine(ShowFirstSecure());
			return;
		}
		switch (currentModal)
		{
		case DialogManager.ModalMode.Modal:
			state = DialogState.WaitPress;
			break;
		case DialogManager.ModalMode.NoModal:
		case DialogManager.ModalMode.Boss:
			state = DialogState.WaitSound;
			timeToWait = ((currentSoundLenght != 0f) ? currentSoundLenght : noModalNextLine);
			break;
		}
	}

	public void DialogButtonPressed()
	{
		if (currentModal == DialogManager.ModalMode.NoModal)
		{
			return;
		}
		switch (state)
		{
		case DialogState.FadeText:
			currentTextAlpha = 1f;
			break;
		case DialogState.WaitPress:
			Core.Dialog.UIEvent_LineEnded();
			break;
		case DialogState.ScrollInitial:
			timeToWait = 0f;
			break;
		case DialogState.ScrollScrolling:
			scrollbarTween.Complete();
			scrollbar.value = 0f;
			break;
		case DialogState.WaitSound:
			if (currentModal == DialogManager.ModalMode.Boss)
			{
				Core.Dialog.UIEvent_LineEnded();
			}
			break;
		case DialogState.WaitResponse:
			break;
		}
	}

	public void SetOptionSelected(int response)
	{
		for (int i = 0; i < 2; i++)
		{
			dialogResponseSelection[i].SetActive(i == response);
			Text componentInChildren = dialogResponseSelection[i].transform.parent.GetComponentInChildren<Text>();
			componentInChildren.color = ((i != response) ? optionNormalColor : optionHighligterColor);
		}
	}

	public void ResponsePressed(int response)
	{
		if (state == DialogState.WaitResponse)
		{
			Core.Dialog.UIEvent_LineEnded(response);
		}
	}

	public bool IsShowingDialog()
	{
		return state != DialogState.Off;
	}

	public int GetNumberOfLines(string cad)
	{
		bool activeSelf = linesRoot.activeSelf;
		linesRoot.SetActive(value: true);
		dialogLine.text = cad;
		Canvas.ForceUpdateCanvases();
		int lineCount = dialogLine.cachedTextGenerator.lineCount;
		linesRoot.SetActive(activeSelf);
		return lineCount;
	}

	public void OnProgrammerSoundSeted(float time)
	{
		if (state != 0 && currentModal != DialogManager.ModalMode.Modal)
		{
			currentSoundLenght = time;
		}
	}

	public void SetBackgound(bool enabled)
	{
		backgorund.SetActive(enabled);
	}

	public void ShowText(string line, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, textCanvas, isBuyMenu: false);
		linesRoot.SetActive(value: true);
		dialogLine.text = line;
	}

	public void ShowLongText(string line, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, longTextCanvas, isBuyMenu: false);
		longLinesRoot.SetActive(value: true);
		dialoglongLine.text = line;
		scrollbar.value = 1f;
		IsLongText = true;
	}

	public void ShowPurge(string purge, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, purgeCanvas, isBuyMenu: false);
		purgeRoot.SetActive(value: true);
		objectFirstText.text = ScriptLocalization.UI_Inventory.TEXT_QUESTION_GIVE_PURGE;
		purgueFirstText.text = ScriptLocalization.UI_Inventory.TEXT_QUESTION_GIVE_PURGE;
		purgueAmount.text = purge;
	}

	public void ShowPurgeGeneric(string text, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, purgeGenericCanvas, isBuyMenu: false);
		purgueGenericText.text = text;
		purgeGenericRoot.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(purgeGenericRoot.GetComponent<RectTransform>());
	}

	public void ShowItem(string caption, Sprite image, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, objectCanvas, isBuyMenu: false);
		objectRoot.SetActive(value: true);
		objectFirstText.text = ScriptLocalization.UI_Inventory.TEXT_QUESTION_GIVE_ITEM;
		objectSecondText.text = caption + "<color=#F8E4C6FF>?</color>";
		objectImage.sprite = image;
	}

	public void ShowBuy(string purge, string caption, string description, Sprite image, List<string> responses, DialogManager.ModalMode modal)
	{
		InternalShow(responses, modal, buyCanvas, isBuyMenu: true);
		buyRoot.SetActive(value: true);
		buyFirstText.text = ScriptLocalization.UI_Inventory.TEXT_QUESTION_BUY_ITEM;
		buyAmount.text = purge;
		buyCaption.text = caption;
		buyDescrption.text = description;
		buyImage.sprite = image;
		customScrollView.NewContentSetted();
	}

	public void Hide(bool hideWidget)
	{
		if (state != 0)
		{
			state = DialogState.Off;
			if (hideWidget)
			{
				animator.SetBool("SHOW_DIALOG", value: false);
			}
			DOTween.Kill("AutoScroll");
			backgorund.SetActive(value: false);
		}
	}

	public IEnumerator ShowFirstSecure()
	{
		yield return new WaitForFixedUpdate();
		EventSystem.current.SetSelectedGameObject(dialogResponse[1].gameObject);
		EventSystem.current.SetSelectedGameObject(dialogResponse[0].gameObject);
		SetOptionSelected(0);
	}

	private void InternalShow(List<string> responses, DialogManager.ModalMode modal, CanvasGroup canvas, bool isBuyMenu)
	{
		linesRoot.SetActive(value: false);
		longLinesRoot.SetActive(value: false);
		objectRoot.SetActive(value: false);
		purgeRoot.SetActive(value: false);
		purgeGenericRoot.SetActive(value: false);
		buyRoot.SetActive(value: false);
		currentCanvas = canvas;
		IsLongText = false;
		currentSoundLenght = 0f;
		if (state == DialogState.Off)
		{
			animator.SetBool("SHOW_DIALOG", value: true);
		}
		state = DialogState.FadeText;
		hasResponses = responses.Count > 0;
		responsesUI.SetActive(value: false);
		for (int i = 0; i < 2; i++)
		{
			if (i < responses.Count)
			{
				dialogResponse[i].gameObject.SetActive(value: true);
				dialogResponse[i].text = responses[i];
			}
			else
			{
				dialogResponse[i].gameObject.SetActive(value: false);
			}
		}
		currentCanvas.alpha = 0f;
		currentTextAlpha = ((responses.Count <= 0) ? 0f : 1f);
		currentModal = modal;
		responseRoot.localPosition = ((!isBuyMenu) ? normalResponsePosition.localPosition : buyResponsePosition.localPosition);
		linesRoot.transform.localPosition = ((!hasResponses) ? linesWithOutResponsePosition.localPosition : linesWithResponsePosition.localPosition);
		longLinesRoot.transform.localPosition = ((!hasResponses) ? linesWithOutResponsePosition.localPosition : linesWithResponsePosition.localPosition);
		backgroundImage.SetActive(!hasResponses && !isBuyMenu);
		backgroundWithResponseImage.SetActive(hasResponses && !isBuyMenu);
	}
}
