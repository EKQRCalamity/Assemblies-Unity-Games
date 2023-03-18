using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using Framework.Managers;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Widgets;

public class CreditsWidget : SerializedMonoBehaviour
{
	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject uguiScrollView;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject uguiScrollRect;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject uguiContent;

	[BoxGroup("Controls", true, false, 0)]
	[SerializeField]
	private GameObject uguiScrollbar;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Image skipMask;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject upArrow;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject downArrow;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Text scrollSpeedText;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject scrollSpeedIndicatorsRoot;

	[BoxGroup("Config", true, false, 0)]
	[SerializeField]
	private bool autoDisable = true;

	[BoxGroup("Config", true, false, 0)]
	[SerializeField]
	[Range(0f, 500f)]
	private float scrollSpeed = 50f;

	[BoxGroup("Config", true, false, 0)]
	[SerializeField]
	private float initialDelay = 0.9f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float timeToSkip = 1.5f;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private float timeAtEndScrollToClose = 2f;

	private Scrollbar scrollbar;

	private Tweener scrollbarTween;

	private float tweenFloat;

	private bool isAutoScrolling;

	private float easeInA;

	private float time;

	private Player rewired;

	private float timePressingSkip;

	private CanvasGroup canvas;

	private const string INITIAL_SCROLL_TWEEN_ID = "InitialScroll";

	private const string NORMAL_SCROLL_TWEEN_ID = "ScrollPro";

	private const string INCREASE_SPEED_TWEEN_ID = "IncreaseScrollSpeedCredits";

	private const string DECREASE_SPEED_TWEEN_ID = "DecreaseScrollSpeedCredits";

	private const string RETURN_TO_NORMAL_SPEED_TWEEN_ID = "ReturnToNormalScrollSpeedCredits";

	private const int VISIBILITY_OFFSET = 32;

	private int screenHeight = Screen.height;

	private readonly Vector3[] elementCorners = new Vector3[4];

	private RectTransform rtContent;

	private RectTransform rtScrollRect;

	private int framesSkipped;

	private float delaySecondsForFastScroll = 1f;

	private int skippedFramesForFastScroll = 15;

	private int skippedFramesForSlowScroll = 30;

	private float axisThreshold = 0.3f;

	private float timeScaleLimit = 64f;

	private bool isScrollingDown;

	public static CreditsWidget instance;

	private RectTransform[] creditsElements;

	public bool HasEnded { get; private set; }

	public float Alpha
	{
		get
		{
			if (canvas == null)
			{
				canvas = GetComponent<CanvasGroup>();
			}
			return canvas.alpha;
		}
		set
		{
			if (canvas == null)
			{
				canvas = GetComponent<CanvasGroup>();
			}
			canvas.alpha = value;
		}
	}

	private void Awake()
	{
		instance = this;
		canvas = GetComponent<CanvasGroup>();
		HasEnded = false;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void OnEnable()
	{
		HasEnded = false;
		Core.Input.SetBlocker("CREDITS", blocking: true);
		if (uguiContent == null || uguiScrollbar == null || uguiScrollRect == null || uguiScrollView == null)
		{
			Debug.Log("Interactive Credits cannot function until you have filled in the required fields on the script component in the inspector.");
			base.enabled = false;
		}
		else
		{
			Invoke("DelayedSetup", 0.1f);
		}
		scrollbar = uguiScrollbar.GetComponent<Scrollbar>();
		scrollbar.enabled = true;
		tweenFloat = 1f;
		screenHeight = Screen.height;
		creditsElements = uguiContent.transform.GetComponentsInChildren<RectTransform>();
	}

	private void OnDisable()
	{
		Vector3 localPosition = uguiContent.transform.localPosition;
		localPosition.y = 0f;
		uguiContent.transform.localPosition = localPosition;
		scrollbarTween.Kill();
		Core.Input.SetBlocker("CREDITS", blocking: false);
	}

	private void Update()
	{
		if (!isAutoScrolling)
		{
			return;
		}
		scrollbar.value = tweenFloat;
		if (rewired == null)
		{
			rewired = ReInput.players.GetPlayer(0);
		}
		if (rewired.GetButton(39))
		{
			timePressingSkip += Time.unscaledDeltaTime;
			if (timePressingSkip >= timeToSkip)
			{
				EndOfCredits();
			}
		}
		else
		{
			timePressingSkip = 0f;
		}
		skipMask.fillAmount = timePressingSkip / timeToSkip;
		float axisRaw = rewired.GetAxisRaw(49);
		if (Mathf.Abs(axisRaw) > axisThreshold)
		{
			ProcessScrollInput(axisRaw);
		}
		RefreshVisibleElements();
	}

	private void RefreshVisibleElements()
	{
		for (int i = 0; i < creditsElements.Length; i++)
		{
			if (!(creditsElements[i].gameObject == uguiContent))
			{
				creditsElements[i].GetWorldCorners(elementCorners);
				float y = elementCorners[1].y;
				float y2 = elementCorners[0].y;
				bool flag = y > -32f && y2 < (float)(screenHeight + 32);
				if (creditsElements[i].gameObject.activeInHierarchy != flag)
				{
					creditsElements[i].gameObject.SetActive(flag);
				}
			}
		}
	}

	private void ProcessScrollInput(float scrollAxis)
	{
		if (scrollbarTween == null)
		{
			return;
		}
		float axisRawPrev = rewired.GetAxisRawPrev(49);
		if (axisRawPrev == 0f)
		{
			framesSkipped = 0;
			if (scrollAxis > 0f)
			{
				ProcessScrollUpwards();
			}
			else
			{
				ProcessScrollDownwards();
			}
			return;
		}
		float axisTimeActive = rewired.GetAxisTimeActive(49);
		int num = ((!(axisTimeActive > delaySecondsForFastScroll)) ? skippedFramesForSlowScroll : skippedFramesForFastScroll);
		framesSkipped++;
		if (framesSkipped % num == 0)
		{
			framesSkipped = 0;
			if (scrollAxis > 0f)
			{
				ProcessScrollUpwards();
			}
			else
			{
				ProcessScrollDownwards();
			}
		}
	}

	private void DelayedSetup()
	{
		Invoke("InitialEaseIn", initialDelay);
		rtScrollRect = uguiScrollView.GetComponent<RectTransform>();
		Vector3 size = new Vector3(rtScrollRect.rect.width, rtScrollRect.rect.height, 1f);
		if (uguiScrollRect.GetComponent<ScrollRect>() != null && uguiScrollRect.GetComponent<BoxCollider>() == null)
		{
			BoxCollider boxCollider = uguiScrollRect.AddComponent<BoxCollider>();
			boxCollider.size = size;
		}
		rtContent = uguiContent.GetComponent<RectTransform>();
		time = (rtContent.rect.height - rtScrollRect.rect.height) / scrollSpeed;
		easeInA = (time - 0.5f) / time;
		VerticalLayoutGroup component = uguiContent.GetComponent<VerticalLayoutGroup>();
		ContentSizeFitter component2 = uguiContent.GetComponent<ContentSizeFitter>();
		if (component != null)
		{
			component.enabled = false;
		}
		if (component2 != null)
		{
			component2.enabled = false;
		}
	}

	private void ProcessScrollDownwards()
	{
		if (scrollbarTween.timeScale == timeScaleLimit && isScrollingDown)
		{
			return;
		}
		float num = 0f;
		if (isScrollingDown)
		{
			num = ((scrollbarTween.timeScale != 0f) ? (scrollbarTween.timeScale * 2f) : 1f);
		}
		else if (scrollbarTween.timeScale > 1f)
		{
			num = 1f;
		}
		else if (scrollbarTween.timeScale == 1f)
		{
			num = 0f;
		}
		else if (scrollbarTween.timeScale == 0f)
		{
			isScrollingDown = true;
			num = 1f;
			if (scrollbarTween.IsBackwards())
			{
				scrollbarTween.PlayForward();
			}
			else
			{
				scrollbarTween.PlayBackwards();
			}
		}
		UpdateScrollSpeedIndicators(num);
		scrollbarTween.DOTimeScale(num, 0f);
	}

	private void ProcessScrollUpwards()
	{
		if (scrollbarTween.timeScale == timeScaleLimit && !isScrollingDown)
		{
			return;
		}
		float num = 0f;
		if (!isScrollingDown)
		{
			num = ((scrollbarTween.timeScale != 0f) ? (scrollbarTween.timeScale * 2f) : 1f);
		}
		else if (scrollbarTween.timeScale > 1f)
		{
			num = 1f;
		}
		else if (scrollbarTween.timeScale == 1f)
		{
			num = 0f;
		}
		else if (scrollbarTween.timeScale == 0f)
		{
			isScrollingDown = false;
			num = 1f;
			if (scrollbarTween.IsBackwards())
			{
				scrollbarTween.PlayForward();
			}
			else
			{
				scrollbarTween.PlayBackwards();
			}
		}
		UpdateScrollSpeedIndicators(num);
		scrollbarTween.DOTimeScale(num, 0f);
	}

	private void UpdateScrollSpeedIndicators(float newTimeScale)
	{
		if (!scrollSpeedIndicatorsRoot.activeInHierarchy)
		{
			scrollSpeedIndicatorsRoot.SetActive(value: true);
		}
		if (newTimeScale == 0f)
		{
			upArrow.SetActive(value: false);
			downArrow.SetActive(value: false);
		}
		else
		{
			upArrow.SetActive(!isScrollingDown);
			downArrow.SetActive(isScrollingDown);
		}
		scrollSpeedText.text = "x" + Mathf.FloorToInt(newTimeScale);
	}

	private void InitialEaseIn()
	{
		tweenFloat = 1f;
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, easeInA, 1f).SetEase(Ease.InQuad).OnComplete(InitialScroll)
			.SetId("InitialScroll");
	}

	private void InitialScroll()
	{
		isScrollingDown = true;
		isAutoScrolling = true;
		tweenFloat = easeInA;
		float duration = time * tweenFloat;
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 0f, duration).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.OnRewind(StartScrollAgain)
			.SetId("ScrollPro");
	}

	private void StartScrollAgain()
	{
		InitialScroll();
		UpdateScrollSpeedIndicators(1f);
	}

	private void ScrollFromAValue(float scrollbarValue)
	{
		isScrollingDown = true;
		tweenFloat = scrollbarValue;
		float duration = time * tweenFloat;
		scrollbarTween.Kill();
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 0f, duration).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.OnRewind(ScrollToStart)
			.SetId("ScrollPro");
		UpdateScrollSpeedIndicators(1f);
	}

	private void ScrollToEnd()
	{
		isScrollingDown = true;
		float timeScale = scrollbarTween.timeScale;
		float duration = time * tweenFloat;
		scrollbarTween.Kill();
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 0f, duration).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.OnRewind(ScrollToStart)
			.SetId("ScrollPro");
		scrollbarTween.ForceInit();
		scrollbarTween.timeScale = timeScale;
		UpdateScrollSpeedIndicators(timeScale);
	}

	private void ScrollToStart()
	{
		isScrollingDown = false;
		float timeScale = scrollbarTween.timeScale;
		float duration = time * (1f - tweenFloat);
		scrollbarTween.Kill();
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 1f, duration).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.OnRewind(ScrollToEnd)
			.SetId("ScrollPro");
		scrollbarTween.ForceInit();
		scrollbarTween.timeScale = timeScale;
		UpdateScrollSpeedIndicators(timeScale);
	}

	private void AutoScrollEnd()
	{
		if (timeAtEndScrollToClose > 0f)
		{
			StartCoroutine(WaitAndEnd());
		}
		else
		{
			EndOfCredits();
		}
	}

	private IEnumerator WaitAndEnd()
	{
		yield return new WaitForSeconds(timeAtEndScrollToClose);
		EndOfCredits();
	}

	private void EndOfCredits()
	{
		Core.Audio.Music.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		isAutoScrolling = false;
		HasEnded = true;
		if (autoDisable)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
