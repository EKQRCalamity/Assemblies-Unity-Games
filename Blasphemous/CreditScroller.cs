using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CreditScroller : MonoBehaviour
{
	public Camera uguiCamera;

	public GameObject uguiScrollView;

	public GameObject uguiScrollRect;

	public GameObject uguiContent;

	public GameObject uguiScrollbar;

	private Scrollbar scrollbar;

	private Tweener scrollbarTween;

	private float tweenFloat;

	private bool isAutoScrolling;

	private bool touchInside;

	private float currentStart;

	private float easeInA;

	private float easeInB;

	private float easeStart;

	private int fingerID;

	private float initialTime;

	private float mathHelperA;

	private float mathHelperB;

	private float percentage;

	private float time;

	private bool letsGo;

	private ScrollRect srComponent;

	[Range(0f, 500f)]
	public float scrollSpeed = 50f;

	public float initialDelay = 0.9f;

	private void Awake()
	{
		Input.multiTouchEnabled = false;
	}

	private void OnEnable()
	{
		letsGo = false;
		if (uguiCamera == null || uguiContent == null || uguiScrollbar == null || uguiScrollRect == null || uguiScrollView == null)
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
		isAutoScrolling = true;
	}

	private void OnDisable()
	{
		Vector3 localPosition = uguiContent.transform.localPosition;
		localPosition.y = 0f;
		uguiContent.transform.localPosition = localPosition;
		DOTween.Complete("ScrollPro");
		DOTween.Kill("ScrollPro");
	}

	private void Update()
	{
		if (letsGo && !touchInside && (double)Mathf.Abs(srComponent.velocity.y) <= 0.1)
		{
			letsGo = false;
			PressReleased();
		}
		if (isAutoScrolling)
		{
			scrollbar.value = tweenFloat;
		}
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = uguiCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray))
			{
				CancelInvoke("PressReleased");
				CancelInvoke("InitialEaseIn");
				scrollbarTween.Kill();
				isAutoScrolling = false;
				touchInside = true;
			}
			else
			{
				touchInside = false;
			}
		}
		if (Input.GetMouseButtonUp(0) && touchInside)
		{
			tweenFloat = scrollbar.value;
			letsGo = true;
			touchInside = false;
		}
	}

	private void DelayedSetup()
	{
		Invoke("InitialEaseIn", initialDelay);
		RectTransform component = uguiScrollView.GetComponent<RectTransform>();
		Vector3 size = new Vector3(component.rect.width, component.rect.height, 1f);
		if (uguiScrollRect.GetComponent<ScrollRect>() != null && uguiScrollRect.GetComponent<BoxCollider>() == null)
		{
			BoxCollider boxCollider = uguiScrollRect.AddComponent<BoxCollider>();
			boxCollider.size = size;
		}
		srComponent = uguiScrollRect.GetComponent<ScrollRect>();
		RectTransform component2 = uguiContent.GetComponent<RectTransform>();
		mathHelperA = component2.rect.height - component.rect.height;
		initialTime = mathHelperA / scrollSpeed;
		mathHelperA /= scrollSpeed / 2f;
		mathHelperB = mathHelperA - 1f;
		easeInA = mathHelperB / mathHelperA;
		easeInB = 1f - easeInA;
		time = initialTime;
		easeStart = 1f;
		currentStart = 1f;
		percentage = 0f;
		VerticalLayoutGroup component3 = uguiContent.GetComponent<VerticalLayoutGroup>();
		ContentSizeFitter component4 = uguiContent.GetComponent<ContentSizeFitter>();
		if (component3 != null)
		{
			component3.enabled = false;
		}
		if (component4 != null)
		{
			component4.enabled = false;
		}
	}

	private void InitialEaseIn()
	{
		tweenFloat = currentStart;
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, easeInA, 1f).SetEase(Ease.InQuad).OnComplete(InitialScroll)
			.SetId("ScrollPro");
	}

	private void InitialScroll()
	{
		tweenFloat = easeInA;
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 0f, time).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.SetId("ScrollPro");
	}

	private void PressReleased()
	{
		currentStart = scrollbar.value;
		time = initialTime;
		percentage = currentStart;
		time *= percentage;
		easeStart = currentStart - easeInB;
		tweenFloat = currentStart;
		isAutoScrolling = true;
		if (tweenFloat != 0f)
		{
			scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
			{
				tweenFloat = x;
			}, easeStart, 1f).SetEase(Ease.InQuad).OnComplete(AutoScrolling)
				.SetId("ScrollPro");
		}
	}

	private void AutoScrolling()
	{
		scrollbarTween = DOTween.To(() => tweenFloat, delegate(float x)
		{
			tweenFloat = x;
		}, 0f, time).SetEase(Ease.Linear).OnComplete(AutoScrollEnd)
			.SetId("ScrollPro");
	}

	private void AutoScrollEnd()
	{
		isAutoScrolling = false;
	}
}
